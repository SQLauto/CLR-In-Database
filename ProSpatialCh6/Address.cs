using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProSpatialCh6
{
    public class Address
    {
            public SqlXml    XMLContent =       null;
            public SqlString AddressLine =      null;    
            public SqlString AdminDistrict =    null;
            public SqlString CountryRegion =    null;
            public SqlString FormattedAddress = null;
            public SqlString Locality =         null;
            public SqlString PostalCode =       null;
            public Address(SqlXml xmlContent, String addressLine, String admindistrict, String countryRegion,
                String formattedAddress, String locality, String postalCode)
            {
                try
                {
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

            public Address(XmlNode xmlNode)
            {
                try
                {
                    using (XmlNodeReader xnr = new XmlNodeReader(xmlNode))
                    {
                        XMLContent = new SqlXml(xnr);
                        AdminDistrict = xmlNode["AdminDistrict"].InnerText;
                        CountryRegion = xmlNode["CountryRegion"].InnerText;
                        FormattedAddress = xmlNode["FormattedAddress"].InnerText;
                        Locality = xmlNode["Locality"].InnerText;
                        PostalCode = xmlNode["PostalCode"].InnerText;
                        AddressLine = xmlNode["AddressLine"].InnerText;
                    }
                }
                catch (Exception)
                {
                    AddressLine = null;
                }
                    
            }
    }
}
