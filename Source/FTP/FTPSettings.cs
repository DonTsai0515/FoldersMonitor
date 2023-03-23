using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace FoldersMonitor
{
    public class Target : ConfigurationElement
    {
        [ConfigurationProperty("Number", IsRequired = true)]
        public int Number => (int)this["Number"];

        [ConfigurationProperty("Host", IsRequired = true)]
        public string Host => (string)this["Host"];

        [ConfigurationProperty("Port", IsRequired = true)]
        public int Port => (int)this["Port"];

        [ConfigurationProperty("UserName", IsRequired = true)]
        public string UserName => (string)this["UserName"];

        [ConfigurationProperty("Password", IsRequired = true)]
        public string Password => (string)this["Password"];

        //EncryptionMode：{"None":"0","Implicit":"1","Explicit":"2","Auto":"3"}        
        [ConfigurationProperty("EncryptionMode", IsRequired = true)]
        public int EncryptionMode => (int)(this["EncryptionMode"]);
    }

    [ConfigurationCollection(typeof(Target))]
    public class FTPTarget : ConfigurationElementCollection
    {
        private const string PropertyName = "Target";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => PropertyName;

        protected override ConfigurationElement CreateNewElement()
        {
            return new Target();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Target)element).Number;
        }

        public Target this[int idx] => (Target)BaseGet(idx);
    }

    public class FTP : ConfigurationSection
    {
        [ConfigurationProperty("FTPTarget")]
        public FTPTarget FTPTarget
        {
            get
            {
                return (FTPTarget)this["FTPTarget"];
            }
            set
            {
                this["FTPTarget"] = value;
            }
        }
    }
}
