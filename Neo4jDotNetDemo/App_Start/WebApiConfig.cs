using iTextSharp.text;
using iTextSharp.text.pdf;
using Neo4jClient;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Http;
using Neo4jDotNetDemo.Controllers;
using Org.BouncyCastle.Pkcs;
using iTextSharp.text.pdf.security;
using System.Net.Http;
using Newtonsoft.Json;

namespace Neo4jDotNetDemo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            
            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            //Use an IoC container and register as a Singleton
            var url = ConfigurationManager.AppSettings["GraphDBUrl"];
            var client = new GraphClient(new Uri("http://neo4j:root@localhost:7474/db/data"));
            client.Connect();
            
            GraphClient = client;
            FormOtherDB();
            var MStream = FormPDF();
            string key = "1";
            SignPdfFile(MStream, new FileStream("E:\\Огойко Микола.p12", FileMode.Open), key);
        }

        public static void FormOtherDB()
        {
            string myclient = "http://localhost:41453/api/";
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            client.BaseAddress = new Uri(myclient);
            response = client.GetAsync("expert").Result;
            List<Expert> expertsList = new List<Expert>();
            if (response.IsSuccessStatusCode)
            {
                expertsList = response.Content.ReadAsAsync<IEnumerable<Expert>>().Result.ToList();
            }
            for (int i = 0; i < expertsList.Count; i++)
            {
                var newExpert = new Expert
                        {
                            idExperts = expertsList[i].idExperts,
                            name = expertsList[i].name,
                            workplace = expertsList[i].workplace,
                            phone = expertsList[i].phone,
                            address = expertsList[i].address
                        };
                GraphClient.Cypher
                .Merge("(expert:Expert { Id: {id} })")
                .OnCreate()
                .Set("expert = {newExpert}")
                .WithParams(new
                {
                    id = newExpert.idExperts,
                    newExpert
                })
                .ExecuteWithoutResults();
                response = client.GetAsync("documents").Result;
                List<documents> documents = new List<documents>();
                if (response.IsSuccessStatusCode)
                {
                    documents = response.Content.ReadAsAsync<IEnumerable<documents>>().Result.ToList();
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
                foreach (var item in documents)
                {
                    if (item.Experts_idExperts == expertsList[i].idExperts)
                    {
                        var newdoc = new documents
                        {
                            idDocuments = item.idDocuments,
                            releaseDate = item.releaseDate,
                            endDate = item.endDate,
                            documentType = item.documentType
                        };
                        GraphClient.Cypher
                        .Merge("(document:Document { id: {id} })")
                        .OnCreate()
                        .Set("document = {newdoc}")
                        .WithParams(new
                        {
                            id = newdoc.idDocuments,
                            newdoc
                        })
                        .ExecuteWithoutResults();
                        GraphClient.Cypher
                        .Match("(expert:Expert)", "(document:Document)")
                        .Where((Expert expert) => expert.idExperts == newExpert.idExperts)
                        .AndWhere((documents document) => document.idDocuments == newdoc.idDocuments)
                        .CreateUnique("expert-[:HAS]->document")
                        .ExecuteWithoutResults();
                    }
                }
            }
            string url = "http://localhost:60200/api/";
            HttpClient client2Other = new HttpClient();
            client2Other.BaseAddress = new Uri(url);
            response = client2Other.GetAsync("notaries").Result;
            List<notary> notaryList = new List<notary>();
            if (response.IsSuccessStatusCode)
            {
                notaryList = response.Content.ReadAsAsync<IEnumerable<notary>>().Result.ToList();
            }
            for (int i = 0; i < notaryList.Count; i++)
            {
                var newnotary = new Notary
                {
                    Id = notaryList[i].idNotary,
                    name = notaryList[i].Name
                };
                GraphClient.Cypher
                       .Merge("(notary:Notary { Id: {id} })")
                       .OnCreate()
                       .Set("notary = {newnotary}")
                       .WithParams(new
                       {
                           id = newnotary.Id,
                           newnotary
                       })
                       .ExecuteWithoutResults();
                GraphClient.Cypher
                                .Match("(expert:Expert)", "(notary:Notary)")
                                .Where("expert.name = \"Коваленко Віктор\"")
                                .AndWhere("notary.name = \"Vasya\"")
                                .CreateUnique("expert-[:HAS_notary]->notary")
                                .ExecuteWithoutResults();
            }
        }

        public static void SignPdfFile(MemoryStream sourceDocument, Stream privateKeyStream, string keyPassword)
        {
            var pk12 = new Pkcs12Store(privateKeyStream, keyPassword.ToCharArray());
            privateKeyStream.Dispose();

            string alias = pk12.Aliases.Cast<string>().FirstOrDefault(pk12.IsKeyEntry);
            var pk = pk12.GetKey(alias).Key;

            Byte[] result;

            var reader = new PdfReader(sourceDocument);
            using (var fout = new MemoryStream())
            {
                using (var stamper = PdfStamper.CreateSignature(reader, fout, '\0'))
                {
                    var appearance = stamper.SignatureAppearance;
                    var ExternalSignature = new PrivateKeySignature(pk, "SHA-512");
                    MakeSignature.SignDetached(appearance, ExternalSignature, new[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, CryptoStandard.CADES);

                    result = fout.ToArray();
                    stamper.Close();
                }
            }
            MemoryStream ms = new MemoryStream(result);
            FileStream fs = new FileStream("E:\\Result.pdf", FileMode.Create, FileAccess.Write);
            ms.WriteTo(fs);
            fs.Close();
            ms.Close();

        }

        private static BaseFont RegisterFonts()
        {
            string[] fontNames = { "Calibri.ttf", "Arial.ttf", "Segoe UI.ttf", "Tahoma.ttf" };
            string fontFile = null;

            foreach (string name in fontNames)
            {
                fontFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), name);
                if (!File.Exists(fontFile))
                {
                    fontFile = null;
                }
                else break;
            }
            if (fontFile == null)
            {
                throw new FileNotFoundException("No fonts!");
            }

            FontFactory.Register(fontFile);
            return BaseFont.CreateFont(fontFile, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
        }
        public static MemoryStream FormPDF()
        {
            var doc = new Document();
            BaseFont baseFont = RegisterFonts();
            var Stream = new MemoryStream();
            var PDFWritet = PdfWriter.GetInstance(doc, Stream);
            doc.Open();
            var ExpertsList = GraphClient.Cypher
                .Match("(expert:Expert)")
                .Return(expert => expert.As<Expert>())
                .Results
                .ToList();
            foreach (var item in ExpertsList)
            {
                Font font14 = new Font(baseFont, 14);
                Font cat = new Font(baseFont, 14, Font.BOLD);
                Font MainFont = new Font(baseFont, 16, Font.BOLD | Font.UNDERLINE);
                doc.Add(new Paragraph("ПІБ: " + item.name, MainFont));
                doc.Add(new Paragraph("Місце роботи: " + item.workplace, font14));
                doc.Add(new Paragraph(""));
                var Doclist = GraphClient.Cypher
                .Match("(expert:Expert)-[HAS_DOC]->(document:Document)")
                .Where((Expert expert) => expert.name == item.name)
                .Return(document => document.As<DocumentsResult>())
                .Results
                .ToList();
                var notlist = GraphClient.Cypher
                .Match("(expert:Expert)-[HAS_notary]->(notary:Notary)")
                .Where((Expert expert) => expert.name == item.name)
                .Return(notary => notary.As<NotaryResult>())
                .Results
                .ToList();
                doc.Add(new Paragraph("Свідоцтва", cat));
                foreach (var document in Doclist)
                {
                    doc.Add(new Paragraph("Дата отримання: " + document.releaseDate, font14));
                    doc.Add(new Paragraph("Закінчення дії: " + document.endDate, font14));
                    doc.Add(new Paragraph("Тип: " + document.DocumentType, font14));
                    doc.Add(Chunk.NEWLINE);
                }
                foreach (var notary in notlist)
                {
                    doc.Add(new Paragraph("Нотаріус: " + notary.name, font14));
                    doc.Add(Chunk.NEWLINE);
                }
                /*document.Add(new Paragraph("", font14));
                 var RealityList = GraphClient.Cypher
                .Match("(expert:Expert)-[owns]->(reality:Rielity)")
                .Where((Expert expert) => expert.Id == item.Id)
                .Return(reality => reality.As<Rielity>())
                .Results
                .ToList();
                document.Add(new Paragraph("Власність", cat));
                foreach (var reality in RealityList)
                {
                    if (reality.RegId == String.Empty
                        || reality.RegId == null)
                    {
                        document.Add(new Paragraph("Нерухомість: ", font14));
                        document.Add(new Paragraph("Адреса: " + reality.Address, font14));
                    }
                    else
                    {
                        document.Add(new Paragraph("Транспортний засіб: ", font14));
                        document.Add(new Paragraph("Серійний номер: " + reality.SerialId, font14));
                        document.Add(new Paragraph("Номер реєстрації: " + reality.RegId, font14));
                    }
                    if (reality.Info == String.Empty)
                    {
                        document.Add(new Paragraph("Додаткова інформація:  " + reality.SerialId, font14));
                    }
                    document.Add(new Paragraph("", font14));
                }
                document.Add(Chunk.NEWLINE);*/
            }
            PDFWritet.CloseStream = false;
            doc.Close();
            Stream.Flush();
            Stream.Position = 0;
            return Stream;
        }
        public static IGraphClient GraphClient { get; private set; }

        public class Notary
        {
            public int Id { get; set; }
            public string name { get; set; }
     
        }

         public partial class documents
    {
        public int idDocuments { get; set; }
        public Nullable<System.DateTime> releaseDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public string documentType { get; set; }
        public int Experts_idExperts { get; set; }
        
        [JsonIgnore]
        public virtual Expert experts { get; set; }
    }

    }
}
