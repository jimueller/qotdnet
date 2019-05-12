using System;
using System.Collections.Generic;
using System.Text;

namespace qotdnet
{
    class Quote
    {
        public string Text { get; set; }
        public string AttributedTo { get; set; }
        public int? Year { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Text);
            sb.Append(" - ");

            if (string.IsNullOrEmpty(AttributedTo))
            {
                sb.Append("Unknown");
            }
            else
            {
                sb.Append(AttributedTo);
            }

            if (!(null == Year))
            {
                sb.Append(" (").Append(Year).Append(")");
            }

            return sb.ToString();
        }
    }
}
