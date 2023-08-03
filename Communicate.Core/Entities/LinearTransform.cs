using System;
using System.Collections.Generic;
using System.Text;

namespace Communicate.Core.Entities
{
    public enum Scaling
    {
        None,
        Linear
    }
    public class LinearTransform
    {
        public decimal x1 { get; set; }
        public decimal y1 { get; set; }
        public decimal x2 { get; set; }
        public decimal y2 { get; set; }

        public object TransValue(object transValue, string OutType)
        {
            try
            {
                decimal value = decimal.Parse(transValue.ToString());
                decimal resulu = 0;
                if (x1 == x2)
                {
                    resulu = value * (y2 / y1);
                }
                else
                {
                    resulu = (y2 - y1) / (x2 - x1) * value - (y2 - y1) / (x2 - x1) * x1 + y1;
                }
                switch (OutType)
                {
                    case "WORD":
                    case "INT":
                        return Math.Round(resulu, MidpointRounding.AwayFromZero);
                    default:
                        return resulu;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        //public static object TransValue(decimal value, decimal x1, decimal y1, decimal x2, decimal y2,string OutType)
        //{
        //    decimal resulu = 0;
        //    if (x1 == x2)
        //    {
        //        resulu = value * (y2 / y1);
        //    }
        //    else
        //    {
        //        resulu = (y2 - y1) / (x2 - x1) * value - (y2 - y1) / (x2 - x1) * x1 + y1;
        //    }
        //    switch (OutType)
        //    {
        //        case "WORD":
        //        case "INT":
        //            return Math.Round(resulu, MidpointRounding.AwayFromZero);
        //        default:
        //            return resulu;
        //    }
        //}
    }
}
