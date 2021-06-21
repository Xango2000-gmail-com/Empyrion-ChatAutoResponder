using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ChatAutoresponder
{
    class Keywords
    {
        public static Root Retrieve(String filePath)
        {
            var input = File.OpenText(filePath);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            var Output = deserializer.Deserialize<Root>(input);
            return Output;
        }

        public class Root
        {
            public List<Responder> Responses { get; set; }
        }

        public class Responder
        {
            public string Keyword { get; set; }
            public string Filename { get; set; }
            public string Type { get; set; }
        }
    }
}
