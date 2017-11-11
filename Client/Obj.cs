using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Obj
    {

        public string customer { get; set; }
        public string wo { get; set; }
        public string model { get; set; }
        //public string hostName { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        // Thời gian bắt đầu gọi AGV
        public string timeCall { get; set; }
        // Thời gian bắt đầu gửi AGV
        public string timeResponseStart { get; set; }
        // Thời gian AGV đến nơi
        public string timeResponseEnd { get; set; }
        public string quantity { get; set; }
    }
    public class objState
    {
        public string name { get; set; }
        public string color { get; set; }
    }
}
