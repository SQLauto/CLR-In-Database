using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProSpatialCh6
{
    public class MarkersAndAddresses
    {
        public SqlXml XMLContent { get; set; }
        public SqlString AddressLine { get; set; }
        public SqlString AdminDistrict { get; set; }
        public SqlString CountryRegion { get; set; }
        public SqlString FormattedAddress { get; set; }
        public SqlString Locality { get; set; }
        public SqlString PostalCode { get; set; }

        public SqlDateTime Date { get; set; }
        public SqlString Latitude { get; set; }
        public SqlString Longitude { get; set; }
        public SqlString Altitude { get; set; }

        public MarkersAndAddresses(DateTime date, String coordinates,
            SqlXml xmlContent, String addressLine, String admindistrict, String countryRegion,
            String formattedAddress, String locality, String postalCode)
        {
            try
            {
                Date = new SqlDateTime(date);
                String[] coordinateList = coordinates.Split(' ');
                Latitude = new SqlString(coordinateList[1]);
                Longitude = new SqlString(coordinateList[0]);
                Altitude = new SqlString(coordinateList[2]);
                XMLContent = xmlContent;
                AdminDistrict = new SqlString(admindistrict);
                CountryRegion = new SqlString(countryRegion);
                FormattedAddress = new SqlString(formattedAddress);
                Locality = new SqlString(locality);
                PostalCode = new SqlString(postalCode);
                AddressLine = new SqlString(addressLine); 
            }
            catch (Exception)
            {
                AddressLine = null;
            }
            
        }

        public MarkersAndAddresses(Marker m, Address a)
        {
            
            try
            {
                Date = m.Date;
                Latitude = m.Latitude;
                Longitude = m.Longitude;
                Altitude = m.Altitude;
                XMLContent = a.XMLContent;
                AdminDistrict = a.AdminDistrict;
                CountryRegion = a.CountryRegion;
                FormattedAddress = a.FormattedAddress;
                Locality = a.Locality;
                PostalCode = a.PostalCode;
                AddressLine = a.AddressLine;
            }
            catch (Exception)
            {
                AddressLine = null;
            }
        }
    }
}
