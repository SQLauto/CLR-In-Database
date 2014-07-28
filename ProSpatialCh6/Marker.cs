using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProSpatialCh6
{
    public class Marker
    {
        public SqlDateTime Date { get; set; }
        public SqlString Latitude { get; set; }
        public SqlString Longitude { get; set; }
        public SqlString Altitude { get; set; }
        public Marker(SqlDateTime Date, SqlString Coordinate)
        {
            this.Date = Date;
            var coord = Coordinate.ToString().Split(' ');
            Latitude = coord[1];
            Longitude = coord[0];
            Altitude = coord[2];
        }
    }
}
