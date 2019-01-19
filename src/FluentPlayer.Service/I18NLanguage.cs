using System.Collections.Generic;

namespace Magentaize.FluentPlayer.Service
{
    public class I18NLanguage
    {
        public string Code { get; }

        public string Name { get; }

        public Dictionary<string, string> Texts { get; set; }

        public I18NLanguage(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Code.Equals(((I18NLanguage)obj).Code);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}