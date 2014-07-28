CLR-In-Database
===============

This project makes CLR file which provides making Geocoding requests in database.

* First we must execute following query to enable CLR objects in database:
```sql
EXEC sp_configure 'clr enabled', '1';   
GO
RECONFIGURE;
GO
```
* Then we execute this query to database trust CLR objects:
```sql
ALTER DATABASE ProSpatial
SET TRUSTWORTHY ON;  
GO
```

* We adding CLR file to database with using following sql:
```sql
CREATE ASSEMBLY Geocoder
FROM 'C:\Spatial\ProSpatialCh6.dll' 
WITH PERMISSION_SET = EXTERNAL_ACCESS;
GO
```
- CLR file has been added. If we can expand *Assemblies* folder in Object Explorer window, we can see **Geocoder** CLR database object:

![alt tag](https://raw.githubusercontent.com/ozcanzaferayan/CLR-In-Database/master/Screenshots/3.png)


* And now we create a stored function which takes input as KML file and creates a table corresponding to that file:
```sql
CREATE FUNCTION dbo.ReadKMLFile( @fileName NVARCHAR(MAX))
RETURNS TABLE([Date] DATETIME,
				Latitude NVARCHAR(10), 
				Longitude NVARCHAR(10), 
				Altitude NVARCHAR(10))
AS EXTERNAL name Geocoder.[ProSpatial.UserDefinedFunctions].ReadKMLFileUDF;
GO
```
* And if we can expand the *Functions* folder in Object Explorer Window, we can see **dbo.ReadKMLFile** function:

![alt tag](https://raw.githubusercontent.com/ozcanzaferayan/CLR-In-Database/master/Screenshots/2.png)

* Our function has been added. Let's create other stored function which takes input as latitude and longitude, and creates an address:
```sql
CREATE FUNCTION dbo.ReverseGeocode( @lat NVARCHAR(MAX), @lng NVARCHAR(MAX))
RETURNS TABLE (XMLContent XML, 
		FormattedAddress NVARCHAR(200), 
		AddressLine  NVARCHAR(200),
		AdminDistrict NVARCHAR(200), 
		CountryRegion NVARCHAR(200), 
		Locality NVARCHAR(200), 
		PostalCode NVARCHAR(200))
AS External name Geocoder.[ProSpatial.UserDefinedFunctions].ReverseGeocodeUDF;
GO
```

* As we made like above, we can add another stored function which takes input as a KML file and creates a table corresponding to returned geocoding service values:
```sql
CREATE FUNCTION dbo.ReadKMLToAddresses( @fileName NVARCHAR(MAX))
RETURNS TABLE([Date] DATETIME,
		Latitude NVARCHAR(10), 
		Longitude NVARCHAR(10), 
		Altitude NVARCHAR(10),
		XMLContent XML,
		FormattedAddress NVARCHAR(200),
		AddressLine NVARCHAR(200), 
		AdminDistrict NVARCHAR(200), 
		CountryRegion NVARCHAR(200), 
		Locality NVARCHAR(200), 
		PostalCode NVARCHAR(200))
AS EXTERNAL name Geocoder.[ProSpatial.UserDefinedFunctions].ReadKMLToAddressesUDF;
GO
```

* If we can followed these steps which in above, after we refresh database from the Object Explorer window, our created functions supposed to like this:

![alt tag](https://raw.githubusercontent.com/ozcanzaferayan/CLR-In-Database/master/Screenshots/4.png)

* We ready to execute our stored functions. Let's execute following query and see results:
```sql
SELECT * FROM dbo.ReverseGeocode('40.093847' ,'26.390387');
```
* Output:
 
![alt tag](https://raw.githubusercontent.com/ozcanzaferayan/CLR-In-Database/master/Screenshots/5.png)

* We ready to execute other stored functions. Let's execute following query and see results:
```sql
SELECT * FROM dbo.ReadKMLToAddresses('D:\yedek\Projeler\LBS\data\simplyKML.kml');
```
* Output:
 
![alt tag](https://raw.githubusercontent.com/ozcanzaferayan/CLR-In-Database/master/Screenshots/6.png)
