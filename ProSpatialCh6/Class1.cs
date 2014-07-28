//"AqRg7HvVzL9R8YZBo60ZjfRQh-H7BukL-gbUmF0-bQWGmz7GmguIk2bQ_9nfbL2c";
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net;
using System.IO;
using System.Configuration;
using System.Xml;
using Microsoft.SqlServer.Types;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using ProSpatialCh6; // Used for List

namespace ProSpatial
{
    public partial class UserDefinedFunctions
    {
        #region ReverseGeocode
        public static XmlDocument ReverseGeocode(String Latitude, String Longitude)
        {
            string BingMapsKey = "AqRg7HvVzL9R8YZBo60ZjfRQh-H7BukL-gbUmF0-bQWGmz7GmguIk2bQ_9nfbL2c";
            // Variable to hold the geocode response
            XmlDocument xmlResponse = new XmlDocument();
            String urlTemplate = "http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?o=xml&key={2}";
            string url = string.Format(urlTemplate,Latitude, Longitude, BingMapsKey);
            try
            {
                // Initialise web request
                HttpWebRequest webrequest = null;
                HttpWebResponse webresponse = null;
                Stream stream = null;
                StreamReader streamReader = null;

                // Make request to the Locations API REST service
                webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.Method = "GET";
                webrequest.ContentLength = 0;

                // Retrieve the response
                webresponse = (HttpWebResponse)webrequest.GetResponse();
                stream = webresponse.GetResponseStream();
                streamReader = new StreamReader(stream);
                xmlResponse.LoadXml(streamReader.ReadToEnd());

                // Clean up
                webresponse.Close();
                stream.Dispose();
                streamReader.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Webrequest hatası oluştu");
            }

            // Return an XMLDocument with the geocoded results 
            return xmlResponse;
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read,
            FillRowMethodName = "ReverseGeocodeUDF_FillRow",
            TableDefinition = "XMLContent xml, FormattedAddress nvarchar(200), AddressLine  nvarchar(200), "+
                "AdminDistrict nvarchar(200), " +
                "CountryRegion nvarchar(200), Locality nvarchar(200), PostalCode nvarchar(200)")]
        public static IEnumerable ReverseGeocodeUDF(SqlString Latitude, SqlString Longitude)
        {
            ArrayList resultCollection = new ArrayList();
            XmlDocument geocodeResponse = new XmlDocument();

            String lat = Latitude.ToString();
            String lng = Longitude.ToString();
            // Attempt to geocode the requested address
            try
            {
                geocodeResponse = ReverseGeocode(
                  (string)lat,
                  (string)lng
                );
            }
            // Failed to geocode the address
            catch (Exception ex)
            {
                SqlContext.Pipe.Send(ex.Message.ToString());
            }

            // Declare the XML namespace used in the geocoded response
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
            nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            // Check that we received a valid response from the geocoding server
            if (geocodeResponse.GetElementsByTagName("StatusCode")[0].InnerText != "200")
            {
                throw new Exception("Didn't get correct response from geocoding server");
            }

            // Retrieve the list of geocoded locations

            //// Create a geography Point instance of the first matching location
            //double Latitude = double.Parse(Locations[0]["Point"]["Latitude"].InnerText);
            //double Longitude = double.Parse(Locations[0]["Point"]["Longitude"].InnerText);
            //SqlGeography Point = SqlGeography.Point(Latitude, Longitude, 4326);
            var locationNode = geocodeResponse.GetElementsByTagName("Address")[0];
            SqlXml xmlNode = null;
            using (XmlNodeReader xnr = new XmlNodeReader(locationNode))
            {
                xmlNode = new SqlXml(xnr);
            }
            Address a = new Address(locationNode);
            resultCollection.Add(a);
            return resultCollection;
        }

        public static void ReverseGeocodeUDF_FillRow(object xmlResult,
            out SqlXml returnedXml,
            out SqlString FormattedAddress, out SqlString AddressLine, out SqlString AdminDistrict, 
            out SqlString CountryRegion, out SqlString Locality, out SqlString PostalCode)
        {
            Address a = (Address)xmlResult;
            returnedXml = a.XMLContent;
            AddressLine =      a.AddressLine;
            AdminDistrict =    a.AdminDistrict;
            CountryRegion =    a.CountryRegion;
            FormattedAddress=  a.FormattedAddress;
            Locality =         a.Locality;
            PostalCode =       a.PostalCode;
        }
        #endregion

        #region KMLFileToAddresses
        public static IEnumerable ReadKMLFileToAddresses(String fileName)
        {
            XmlDocument xml = ReadKMLFile(fileName);
            var dateNodeList = xml.GetElementsByTagName("when");
            var coordNodeList = xml.GetElementsByTagName("gx:coord");
            Marker tempMarker = null;
            Address tempAddress = null;
            XmlDocument tempXML = null;
            ArrayList addressAndMarkers = new ArrayList();
            for (int i = 0; i < dateNodeList.Count; i++)
            {
                tempMarker = new Marker(Convert.ToDateTime(dateNodeList[i].InnerText), coordNodeList[i].InnerText);
                tempXML = ReverseGeocode(tempMarker.Latitude.Value, tempMarker.Longitude.Value);
                tempAddress = new Address(tempXML.GetElementsByTagName("Address")[0]);
                addressAndMarkers.Add(new MarkersAndAddresses(tempMarker, tempAddress));
            }
            return addressAndMarkers;
        }
        
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read,
            FillRowMethodName = "ReadKMLToAddresses_FillRow",
            TableDefinition = "Date datetime, Latitude nvarchar(10), Longitude nvarchar(10), " +
                "Altitude nvarchar(10), XMLContent xml, FormattedAddress nvarchar(200), " +  
                "AddressLine  nvarchar(200), AdminDistrict nvarchar(200), " +
                "CountryRegion nvarchar(200), Locality nvarchar(200), PostalCode nvarchar(200)")]
        public static IEnumerable ReadKMLToAddressesUDF(SqlString FileName)
        {
            ArrayList addressesAndMarkers = (ArrayList)ReadKMLFileToAddresses(FileName.ToString());
            return addressesAndMarkers;
        }

        public static void ReadKMLToAddresses_FillRow(object addrm,
            out SqlDateTime Date, out SqlString Latitude, out SqlString Longitude, out SqlString Altitude,
            out SqlXml returnedXml,
            out SqlString FormattedAddress, out SqlString AddressLine, out SqlString AdminDistrict,
            out SqlString CountryRegion, out SqlString Locality, out SqlString PostalCode)
        {
            MarkersAndAddresses a = (MarkersAndAddresses)addrm;
            Date = a.Date;
            Latitude = a.Latitude;
            Longitude = a.Longitude;
            Altitude = a.Altitude;
            returnedXml = a.XMLContent;
            AddressLine = a.AddressLine;
            AdminDistrict = a.AdminDistrict;
            CountryRegion = a.CountryRegion;
            FormattedAddress = a.FormattedAddress;
            Locality = a.Locality;
            PostalCode = a.PostalCode;
        }
        #endregion

        #region ReadingKMLFile
        #region ReadKML file to XMLDocument
        public static XmlDocument ReadKMLFile(String fileName)
        {
            XmlDocument xml = null;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                xml = new XmlDocument();
                StreamReader sr = new StreamReader(fs);
                xml.LoadXml(sr.ReadToEnd());
                fs.Dispose();
                sr.Dispose();
            }
            catch (Exception)
            {

                throw new Exception("XML dosyası getirilemedi");
            }
           
            return xml;
        }
        #endregion

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read,
            FillRowMethodName = "ReadKMLFileUDF_FillRow",
            TableDefinition = "Date datetime, Latitude nvarchar(10), Longitude nvarchar(10), " +
                "Altitude nvarchar(10)")]
        public static IEnumerable ReadKMLFileUDF(SqlString FileName)
        {
            XmlDocument xml = ReadKMLFile(FileName.ToString());
            var dateNodeList = xml.GetElementsByTagName("when");
            var coordNodeList = xml.GetElementsByTagName("gx:coord");
            ArrayList markerList = new ArrayList();
            for (int i = 0; i < dateNodeList.Count; i++)
            {
                markerList.Add(new Marker(Convert.ToDateTime(dateNodeList[i].InnerText), coordNodeList[i].InnerText));
            }
            return markerList;
        }

        public static void ReadKMLFileUDF_FillRow(object Marker,
            out SqlDateTime Date, out SqlString Latitude, out SqlString Longitude, out SqlString Altitude
            )
        {
            Marker m = (Marker)Marker;
            Date = m.Date;
            Latitude = m.Latitude;
            Longitude = m.Longitude;
            Altitude = m.Altitude;
        }
        #endregion










        //[Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        //public static SqlXml ReverseGeocodeUDF(SqlString Latitude, SqlString Longitude)
        //{
        //    XmlDocument geocodeResponse = new XmlDocument();

        //    String lat = Latitude.ToString();
        //    String lng = Longitude.ToString();
        //    // Attempt to geocode the requested address
        //    try
        //    {
        //        geocodeResponse = ReverseGeocode(
        //          (string)lat,
        //          (string)lng
        //        );
        //    }
        //    // Failed to geocode the address
        //    catch (Exception ex)
        //    {
        //        SqlContext.Pipe.Send(ex.Message.ToString());
        //    }

        //    // Declare the XML namespace used in the geocoded response
        //    XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
        //    nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/search/local/ws/rest/v1");

        //    // Check that we received a valid response from the geocoding server
        //    if (geocodeResponse.GetElementsByTagName("StatusCode")[0].InnerText != "200")
        //    {
        //        throw new Exception("Didn't get correct response from geocoding server");
        //    }

        //    // Retrieve the list of geocoded locations

        //    //// Create a geography Point instance of the first matching location
        //    //double Latitude = double.Parse(Locations[0]["Point"]["Latitude"].InnerText);
        //    //double Longitude = double.Parse(Locations[0]["Point"]["Longitude"].InnerText);
        //    //SqlGeography Point = SqlGeography.Point(Latitude, Longitude, 4326);
        //    SqlXml returnedXml = null;
        //    var locationNode = geocodeResponse.GetElementsByTagName("Location")[0];
        //    using (XmlNodeReader xnr = new XmlNodeReader(locationNode))
        //    {
        //        returnedXml = new SqlXml(xnr);
        //    }

        //    return returnedXml;
        //}
    }

}