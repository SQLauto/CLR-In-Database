-- Database'in CLR kütüphanelerini çalıştırabilmesi için
EXEC sp_configure 'clr enabled', '1';   
GO
RECONFIGURE;
GO

-- CLR desteklemesi için gereken izini veriyoruz
ALTER DATABASE ProSpatial SET TRUSTWORTHY ON;  
GO

-- DLL dosyasının konumunu vererek veritabanına yüklüyoruz
CREATE ASSEMBLY Geocoder
FROM 'C:\Spatial\ProSpatialCh6.dll' 
WITH PERMISSION_SET = EXTERNAL_ACCESS;
GO

-- KML dosyasının path'ini input olarak alıyor.
-- date, lat, lng, altitude bilgilerini tablo olarak veriyor 
Create Function dbo.ReadKMLFile( @fileName nvarchar(max))
Returns Table([Date] Datetime, Latitude nvarchar(10), Longitude nvarchar(10), Altitude nvarchar(10))
As External name Geocoder.[ProSpatial.UserDefinedFunctions].ReadKMLFileUDF;
GO

-- Latitude , Longitude bilgilerini alıyor
-- ReverseGeoreference yaparak adresi geri döndürüyor
Create Function dbo.ReverseGeocode( @lat nvarchar(max), @lng nvarchar(max))
Returns Table(XMLContent xml, FormattedAddress nvarchar(200), AddressLine  nvarchar(200),
                AdminDistrict nvarchar(200), CountryRegion nvarchar(200), Locality nvarchar(200), PostalCode nvarchar(200))
As External name Geocoder.[ProSpatial.UserDefinedFunctions].ReverseGeocodeUDF;
GO

-- KML dosya path'ini parametre olarak alıyor
-- Dosyadaki her bir lat lng değeri için reversegeoreference yapıyor
-- Adres bilgilerini döndürüyor
-- Not: Bu fonksiyon üstteki iki fonksiyonun birleşimi gibi çalışıyor
Create Function dbo.ReadKMLToAddresses( @fileName nvarchar(max))
Returns Table(
	[Date] datetime, Latitude nvarchar(10), Longitude nvarchar(10), Altitude nvarchar(10),
	XMLContent xml, FormattedAddress nvarchar(200), AddressLine nvarchar(200), 
	AdminDistrict nvarchar(200), CountryRegion nvarchar(200), Locality nvarchar(200), PostalCode nvarchar(200))
As External name Geocoder.[ProSpatial.UserDefinedFunctions].ReadKMLToAddressesUDF;
GO

USE PROSpatial
Select * from dbo.ReadKMLToAddresses('D:\yedek\Projeler\LBS\data\06 04 2012 10_00 - Copy.kml');
Select * from dbo.ReadKMLToAddresses('D:\yedek\Projeler\LBS\data\simplyKML.kml');

Select * from dbo.ReverseGeocode('40.093847' ,'26.390387');
Select * from dbo.ReadKMLFile('D:\yedek\Projeler\LBS\data\simplyKML.kml');

