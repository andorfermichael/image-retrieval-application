using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using System.Collections;
using System.Xml.Serialization;
using System.Xml;
//using System.IO.Stream;

using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;

using FSDirectory = Lucene.Net.Store.FSDirectory;
using Version = Lucene.Net.Util.Version;

using Accord;
using Accord.Imaging;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics.Kernels;
using AForge;
//using Accord.Math.Distances;

namespace Image_Retrieval_Application
{
    public class CustomSearcher
    {
        public static List<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                try
                {
                    return Directory.GetDirectories(path, searchPattern).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Fatal! Unable to GetDirectories from path {0}! Error: {1}", path, e.Message);
                    return new List<string>();
                }

            }


            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Warning! Unauthorized Access Exception was thrown! Access to {0} was denied!", path);
                return new List<string>();
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal! Reading Directory '{0}' resultet in following Error: {1}", path, e.Message);
                return new List<string>();
            }
        }
    }

    public class DynamicXml : DynamicObject
    {
        XElement _root;
        public DynamicXml(XElement root)
        {
            _root = root;
        }

        public static DynamicXml Parse(string xmlString)
        {
            dynamic DynamicXml = new ExpandoObject();
            return new DynamicXml(XDocument.Parse(xmlString).Root);
        }

        public static DynamicXml Load(string filename)
        {
            return new DynamicXml(XDocument.Load(filename).Root);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            var att = _root.Attribute(binder.Name);
            if (att != null)
            {
                result = att.Value;
                return true;
            }

            var nodes = _root.Elements(binder.Name);
            if (nodes.Count() > 1)
            {
                result = nodes.Select(n => new DynamicXml(n)).ToList();
                return true;
            }

            var node = _root.Element(binder.Name);
            if (node != null)
            {
                if (node.HasElements)
                {
                    //result = new DynamicXml(node);
                    result = new List<DynamicXml>() { new DynamicXml(node) };
                }
                else
                {
                    result = node.Value;
                }
                return true;
            }

            return true;
        }

    }

    static class Program
    {
        internal static readonly DirectoryInfo INDEX_DIR = new DirectoryInfo("index");

        //GLOBAL VARIABLES:
        public static string computedTargetPath;
        public static string projectPath = @"C:\Users\Nico\Downloads\DMS";
        public static List<String> stopWords = new List<String> { "able", "about", "above", "abroad", "according", "accordingly", "across", "actually", "adj", "after", "again", "afterwards", "against", "ago", "ahead", "ain't", "all", "allow", "allows", "almost", "alone", "along", "alongside", "already", "also", "although", "always", "am", "amid", "amidst", "among", "amongst", "an", "and", "another", "any", "anybody", "anyhow", "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "appreciate", "appropriate", "are", "aren't", "around", "as", "a's", "aside", "ask", "asking", "associated", "at", "available", "away", "awfully", "back", "backward", "backwards", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "begin", "behind", "being", "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "both", "brief", "but", "by", "came", "can", "cannot", "cant", "can't", "caption", "cause", "causes", "certain", "certainly", "changes", "clearly", "c'mon", "co", "co.", "com", "come", "comes", "concerning", "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could", "couldn't", "course", "c's", "currently", "dare", "daren't", "definitely", "described", "despite", "did", "didn't", "different", "directly", "do", "does", "doesn't", "doing", "done", "don't", "down", "downwards", "during", "each", "edu", "eg", "eight", "eighty", "either", "else", "elsewhere", "end", "ending", "enough", "entirely", "especially", "et", "etc", "even", "ever", "evermore", "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except", "fairly", "far", "farther", "few", "fewer", "fifth", "first", "five", "followed", "following", "follows", "for", "forever", "former", "formerly", "forth", "forward", "found", "four", "from", "further", "furthermore", "get", "gets", "getting", "given", "gives", "go", "goes", "going", "gone", "got", "gotten", "greetings", "had", "hadn't", "half", "happens", "hardly", "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll", "hello", "help", "hence", "her", "here", "hereafter", "hereby", "herein", "here's", "hereupon", "hers", "herself", "he's", "hi", "him", "himself", "his", "hither", "hopefully", "how", "howbeit", "however", "hundred", "i'd", "ie", "if", "ignored", "i'll", "i'm", "immediate", "in", "inasmuch", "inc", "inc.", "indeed", "indicate", "indicated", "indicates", "inner", "inside", "insofar", "instead", "into", "inward", "is", "isn't", "it", "it'd", "it'll", "its", "it's", "itself", "i've", "just", "k", "keep", "keeps", "kept", "know", "known", "knows", "last", "lately", "later", "latter", "latterly", "least", "less", "lest", "let", "let's", "like", "liked", "likely", "likewise", "little", "look", "looking", "looks", "low", "lower", "ltd", "made", "mainly", "make", "makes", "many", "may", "maybe", "mayn't", "me", "mean", "meantime", "meanwhile", "merely", "might", "mightn't", "mine", "minus", "miss", "more", "moreover", "most", "mostly", "mr", "mrs", "much", "must", "mustn't", "my", "myself", "name", "namely", "nd", "near", "nearly", "necessary", "need", "needn't", "needs", "neither", "never", "neverf", "neverless", "nevertheless", "new", "next", "nine", "ninety", "no", "nobody", "non", "none", "nonetheless", "noone", "no-one", "nor", "normally", "not", "nothing", "notwithstanding", "novel", "now", "nowhere", "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "one's", "only", "onto", "opposite", "or", "other", "others", "otherwise", "ought", "oughtn't", "our", "ours", "ourselves", "out", "outside", "over", "overall", "own", "particular", "particularly", "past", "per", "perhaps", "placed", "please", "plus", "possible", "presumably", "probably", "provided", "provides", "que", "quite", "qv", "rather", "rd", "re", "really", "reasonably", "recent", "recently", "regarding", "regardless", "regards", "relatively", "respectively", "right", "round", "said", "same", "saw", "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed", "seeming", "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "shall", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "since", "six", "so", "some", "somebody", "someday", "somehow", "someone", "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "specified", "specify", "specifying", "still", "sub", "such", "sup", "sure", "take", "taken", "taking", "tell", "tends", "th", "than", "thank", "thanks", "thanx", "that", "that'll", "thats", "that's", "that've", "the", "their", "theirs", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "there'd", "therefore", "therein", "there'll", "there're", "theres", "there's", "thereupon", "there've", "these", "they", "they'd", "they'll", "they're", "they've", "thing", "things", "think", "third", "thirty", "this", "thorough", "thoroughly", "those", "though", "three", "through", "throughout", "thru", "thus", "till", "to", "together", "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying", "t's", "twice", "two", "un", "under", "underneath", "undoing", "unfortunately", "unless", "unlike", "unlikely", "until", "unto", "up", "upon", "upwards", "us", "use", "used", "useful", "uses", "using", "usually", "v", "value", "various", "versus", "very", "via", "viz", "vs", "want", "wants", "was", "wasn't", "way", "we", "we'd", "welcome", "well", "we'll", "went", "were", "we're", "weren't", "we've", "what", "whatever", "what'll", "what's", "what've", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "where's", "whereupon", "wherever", "whether", "which", "whichever", "while", "whilst", "whither", "who", "who'd", "whoever", "whole", "who'll", "whom", "whomever", "who's", "whose", "why", "will", "willing", "wish", "with", "within", "without", "wonder", "won't", "would", "wouldn't", "yes", "yet", "you", "you'd", "you'll", "your", "you're", "yours", "yourself", "yourselves", "you've", "zero" };
        public static List<String> propertiesToIndex = new List<string> { "date_taken", "description", "tags", "title", "username" };
        public static Dictionary<string, int> weightOfPorperties = new Dictionary<string, int>() {
            {"date_taken",1 },
            {"description",2 },
            {"tags",5 },
            {"title",10 },
            {"username",4 }
        };
        public static Dictionary<string, Dictionary<string, int>> searchIndex = new Dictionary<string, Dictionary<string, int>>(); //TODO searchINdex auch xmln
        public static Dictionary<string, string> fileIndex = new Dictionary<string, string>();
        public static Dictionary<string, decimal[]> picFeatures = new Dictionary<string, decimal[]>();

        //GLOBAL VARIABLES

        // TODO: In production replace switch from solutionDirectory to applicationDirectory
        //string applicationDirectory = Path.GetDirectoryName("../../" + Application.ExecutablePath);
        static string solutionDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));

        public static List<string> startTagSearch(string searchValue) {
            Debug.WriteLine("Search Value: " + searchValue.ToLower());
            return getImgPathsForWordSearch(searchFor(searchValue.ToLower()));
        }

        //public static List<string> startSimilaritySearch(Dictionary<string, double> DistancesCollection)
        //{
        //    List<string> resultDistancesPath;

        //    resultDistancesPath = getImgPathsForImageSearch(DistancesCollection);   
            
        //    return resultDistancesPath;
        //}


        public static void startQueryByExampleSearch(string imageLocation)
        {
            Debug.WriteLine("Example Location: " + imageLocation);
        }

        private static List<String> getSubDirectories(string directorypath)
        {
            Console.WriteLine("#reading Directory: " + directorypath);
            var directories = CustomSearcher.GetDirectories(directorypath);
            return directories;
        }

        private static List<String> genXmlFilepaths(List<String> imgSubDirectories)
        {
            List<String> xmlFilepaths = new List<string>();
            foreach (string item in imgSubDirectories)
            {
                // WARNING, this is only valid if Folders are named "img" and "xml" (Length of 3 characters)
                //string tmp = item.Substring(item.IndexOf("img/"));
                xmlFilepaths.Add(item.Replace(@"\img\", @"\xml\") + ".xml");
            }
            return xmlFilepaths;
        }

        private static List<String> genCSVFilepaths(List<String> imgSubDirectories)
        {
            List<String> csvFilepaths = new List<string>();
            foreach (string item in imgSubDirectories)
            {
                // WARNING, this is only valid if Folders are named "img" and "xml" (Length of 3 characters)
                //string tmp = item.Substring(item.IndexOf("img/"));
                csvFilepaths.Add(item.Replace(@"\img\", @"\descvis\descvis\img\") + " CM.csv");
            }
            return csvFilepaths;
        }

        private static  Dictionary<string, decimal[]> saveFeaturesFromCSV(List<String>csvFiles)
        {
            Dictionary<string, decimal[]> resultFeatures = new Dictionary<string, decimal[]>();
            for (int i = 0; i < csvFiles.Count; i++)
			{
			  var reader = new StreamReader(File.OpenRead(csvFiles[i]));
              while (!reader.EndOfStream)
              {
                    var line = reader.ReadLine();
                    if (line.Length > 0)
                    {
                        string[] values = line.Split(',');
                        decimal[] featuresPerLine = new decimal[values.Length - 1];

                        for (int j = 1; j < values.Length; j++)
                        {   
                            featuresPerLine[j-1] = Decimal.Parse(values[j].Replace(".", ","), System.Globalization.NumberStyles.Float);
                            //Console.WriteLine((featuresPerLine[j-1]));
                        }
                        try
                        {
                            resultFeatures.Add(values[0], featuresPerLine);
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Entry skipped");
                        }
                    }
                }
			}
            return resultFeatures;
        }


        private static void addItemToIndex(dynamic Item)
        {
            //  { "date_taken", "description", "tags", "title", "username" }
            foreach (string word in splitIntoWords(Item.date_taken))
                addWordToIndex(Item.id, word.ToLower(), weightOfPorperties["date_taken"]);
            foreach (string word in splitIntoWords(Item.description))
                addWordToIndex(Item.id, word.ToLower(), weightOfPorperties["description"]);
            foreach (string word in splitIntoWords(Item.tags))
                addWordToIndex(Item.id, word.ToLower(), weightOfPorperties["tags"]);
            foreach (string word in splitIntoWords(Item.title))
                addWordToIndex(Item.id, word.ToLower(), weightOfPorperties["title"]);
            foreach (string word in splitIntoWords(Item.username))
                addWordToIndex(Item.id, word.ToLower(), weightOfPorperties["username"]);
        }

        private static List<String> splitIntoWords(string completeString)
        {
            List<String> words = Regex.Matches(completeString, "\\w+")
              .OfType<Match>()
              .Select(m => m.Value)
              .ToList();
            return words;
        }

        public static void addWordToIndex(string ItemID, string Property, int Weight)
        {
            //{ "date_taken", "description", "tags", "title", "username" };
            if (!(stopWords.Contains(Property)) && (Property.Length >= 3))
            {   //check if it is a stopWord, if not, add it to searchIndex
                if (!(searchIndex.ContainsKey(Property)))
                {   //check if Key exists already in Dictionary - if not:
                    searchIndex.Add(Property, new Dictionary<string, int> { { ItemID, Weight } });
                    //Console.WriteLine("# Adding Key '{0}' with Item '{1}'#\n", word, Item.id);
                    Console.Write(".");
                }
                else
                {   //key exists already
                    //Console.WriteLine("## Key '{0}' already exists, adding Item '{1}' to set#\n", word, Item.id);
                    if (!(searchIndex[Property].ContainsKey(ItemID)))
                    {    //check if Item has already been added to indexed word/property - if not add it with current score
                        searchIndex[Property].Add(ItemID, Weight);
                    }
                    else
                    {
                        //item is already in list of indexed word/property, add scorepoints
                        //Console.WriteLine("Current Score of word " + Property + ": " + searchIndex[Property][Item]);
                        searchIndex[Property][ItemID] += Weight;
                    }
                    Console.Write(".");
                }
            }
        }

        private static dynamic parseXML(string filepath)
        {
            Console.WriteLine("parsing XML: " + filepath);
            string xml = File.ReadAllText(filepath);

            dynamic elements = DynamicXml.Parse(xml);
            return elements;
        }

        private static void createFileIndex(string directory, dynamic elements)
        {
            directory = directory.Replace(@"\xml\", @"\img\").Substring(0, directory.Length - 4) + @"\";
            foreach (dynamic item in elements.photo)
            {
                fileIndex.Add(item.id, directory + item.id + ".jpg");
                //Console.WriteLine(directory + item.id + ".jpg");
                string picPath = (string) directory + item.id + ".jpg";
                //Console.WriteLine(picPath);
                
            }
        }

        private static Dictionary<string,int> searchFor(string searchString)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            Dictionary<string, int> resultsSingleQuery;

            foreach (string word in splitIntoWords(searchString))
            {
                resultsSingleQuery = new Dictionary<string, int>();
                try
                {
                    foreach (KeyValuePair<string, int> entry in searchIndex[word])
                    {
                        resultsSingleQuery.Add(entry.Key, entry.Value);
                    }
                    if (results.Count == 0)
                    {
                        foreach (KeyValuePair<string, int> entry in resultsSingleQuery)
                        {
                            results.Add(entry.Key, entry.Value);
                        }
                    }
                    else
                    {
                        results = results.Where(x => resultsSingleQuery.ContainsKey(x.Key))
                                 .ToDictionary(x => x.Key, x => x.Value);
                    }
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine("# Search for keyword '{0}' threw 0 results!",word);
                }
                catch (Exception e)
                {
                    Console.WriteLine("# Error! Following Error ocurred: " + e.Message);
                }
               
            }
            return results.OrderByDescending(key => key.Value).ToDictionary(key => key.Key, key => key.Value);

        }

        private static List<string> getImgPathsForWordSearch(Dictionary<string,int> results)
        {
            List<string> matchingImages = new List<string>();
            foreach (KeyValuePair<string, int> item in results)
                matchingImages.Add(fileIndex[item.Key]);
            return matchingImages;
        }

        private static List<string> getImgPathsForImageSearch(Dictionary<string, double> results)
        {
            List<string> matchingImages = new List<string>();
            foreach (var item in results)
            {
                matchingImages.Add(fileIndex[item.Key]);
            }
            return matchingImages;
        }

        private static List<KeyValuePair<dynamic,int>> paginate(List<KeyValuePair<dynamic, int>> results, int page, int resultsPerPage)
        {
            if (page == 0)
                page = 1;
            else if (page < 0)
                throw new ArgumentException();
             
            List<KeyValuePair<dynamic, int>> resultsForPage = new List<KeyValuePair<dynamic, int>>();
            for (int i = 0 + (page - 1) * resultsPerPage; i < (page * resultsPerPage) && i < results.Count; i++)
            {
                resultsForPage.Add(results[i]);
            }
            Console.WriteLine("Showing results {0} to {1} of {2} on page {3}", (0 + (page - 1) * resultsPerPage), (page * resultsPerPage),results.Count, page);
            return resultsForPage;
        }

        private static XElement generateFeaturePointsXML(Dictionary<string, decimal[]> features)
        {
            XElement root = new XElement("root");
            foreach (KeyValuePair<string, decimal[]> entry in features)
            {
                XElement image = new XElement("image");
                XAttribute id = new XAttribute("id", entry.Key);
                image.Add(id);
                foreach (Decimal feature in entry.Value)
                {
                    XElement element = new XElement("feature");
                    XAttribute value = new XAttribute("value", feature);
                    element.Add(value);
                    image.Add(element);
                }
                root.Add(image);
            }
            return root;
        }

        private static XElement generateFileIndexXML()
        {
            XElement root = new XElement("root");
            foreach (KeyValuePair<string, string> entry in fileIndex)
            {
                XElement image = new XElement("image");
                XAttribute id = new XAttribute("id", entry.Key);
                image.Add(id);
                XAttribute path = new XAttribute("path", entry.Value);
                image.Add(path);
                root.Add(image);
            }
            return root;
        }

        private static XElement generateSearchIndexXML(Dictionary<string, Dictionary<string, int>> index)
        {
            XElement root = new XElement("root");
            foreach (KeyValuePair<string, Dictionary<string, int>> entry in index)
            {
                XElement keyword = new XElement("keyword");
                XAttribute value = new XAttribute("value", entry.Key);
                keyword.Add(value);

                foreach (KeyValuePair<string, int> subentry in entry.Value.OrderByDescending(key => key.Value))
                {
                    XElement image = new XElement("image");
                    XAttribute id = new XAttribute("id", subentry.Key);
                    XAttribute score = new XAttribute("score", subentry.Value);
                    image.Add(id);
                    image.Add(score);
                    keyword.Add(image);
                }
                root.Add(keyword);
            }
            return root;
        }

        public static Dictionary<string, double> computeDistance(string imgID, Dictionary<string, decimal[]> FeatureCollection)  {                 
                       
            //Extract features for SearchedImage
            decimal[] searchedImageFeatures = FeatureCollection["135114980"]; //should be imgID
            Dictionary<string, double> resultDistances = new Dictionary<string, double>();     

			// Calculate Image Similarities
            foreach (var item in FeatureCollection)
            {
                double dist = Distance.Cosine(Array.ConvertAll(searchedImageFeatures, x => (double)x), Array.ConvertAll(item.Value, x => (double)x));
                //Console.WriteLine(dist.ToString());
                //Console.WriteLine(item.Key);
                resultDistances.Add(item.Key, dist);
            }

            return resultDistances.OrderBy(key => key.Value).Take(10).ToDictionary(key => key.Key, key => key.Value);
        }

        public static void initiateComputation()
        {
            Console.WriteLine("### Grabbing IMG and XML-Files ###\n");

            List<String> imgSubDirectories = new List<string>();
            List<String> xmlFiles = new List<string>();
            List<String> csvFilesCM = new List<string>();
            List<String> csvFilesCSD = new List<string>();
            List<String> csvFilesLPB = new List<string>();
            List<String> csvFilesHOG = new List<string>();

            while (xmlFiles.Count == 0 || csvFilesCM.Count == 0 || csvFilesCSD.Count == 0 || csvFilesLPB.Count == 0 || csvFilesHOG.Count == 0)
            {
                try
                {
                    imgSubDirectories = getSubDirectories(projectPath + @"\img");
                    xmlFiles = genXmlFilepaths(imgSubDirectories);
                    csvFilesCM = genCSVFilepaths(imgSubDirectories);
                    csvFilesCSD = genCSVFilepaths(imgSubDirectories);
                    csvFilesLPB = genCSVFilepaths(imgSubDirectories);
                    csvFilesHOG = genCSVFilepaths(imgSubDirectories);
                    computedTargetPath = projectPath + @"\computed\";
                }
                catch (Exception)
                {
                    Console.WriteLine("Project Directory does not contain an neccessary directory/seems to be empty or cannot be found/accessed: '{0}'\n{1}\n\nEnter new absolute Path to Project-Directory:\n(Project Directory must contain following Directories: 'xml' - XML-Metadatas,'img' - Images and 'descvis' - Featurepoints)\t", projectPath);
                    string newProjectPath = Console.ReadLine();
                    //xmlDirectory = newXmlPath;    //not neccessary
                    Console.WriteLine("- Project-Path set to '{0}'", newProjectPath);
                    string newImgPath = Console.ReadLine();
                    Console.WriteLine("\nTrying to fetch Data with new paths...");
                    if (!newProjectPath.EndsWith(@"\"))
                    {
                        newProjectPath = newProjectPath + @"\";
                    }
                    computedTargetPath = projectPath + @"\computed\";
                    imgSubDirectories = getSubDirectories(newProjectPath + @"img");
                    xmlFiles = genXmlFilepaths(imgSubDirectories);
                    csvFilesCM = genCSVFilepaths(imgSubDirectories);
                    csvFilesCSD = genCSVFilepaths(imgSubDirectories);
                    csvFilesLPB = genCSVFilepaths(imgSubDirectories);
                    csvFilesHOG = genCSVFilepaths(imgSubDirectories);
                }
            }

            Console.WriteLine("### Parsing XML-File(s) ###\n");
            System.IO.Directory.CreateDirectory(computedTargetPath);

            Console.WriteLine("### Creating Index ####");
            foreach (string file in xmlFiles)
            {
                dynamic elements = parseXML(file);
                foreach (var photo in elements.photo)
                {
                    addItemToIndex(photo);
                }
                createFileIndex(file, elements);
            }

            Console.WriteLine(fileIndex);
            generateFileIndexXML().Save(computedTargetPath + @"\fileIndex.xml");
            Console.WriteLine("\n\n### Index creation successful! ###\n\n");

            generateSearchIndexXML(searchIndex).Save(computedTargetPath + @"\searchIndex.xml");
            Console.WriteLine("\n\n### Index saved to {0} ###\n\n", computedTargetPath + @"\searchIndex.xml");

            Console.WriteLine("\n\n### Extracting Featurepoints ###\n\n");
            //picFeatures = saveFeaturesFromCSV(csvFiles); //TODO ADD ALL CSVs mit key zb CM
            generateFeaturePointsXML(picFeatures).Save(computedTargetPath + @"\features.xml");
            Console.WriteLine("\n\n### Saved Featurepoints to {0} ###\n\n", computedTargetPath + @"\features.xml");
        }

        public static void loadPrecomputedSearchIndex()
        {
            dynamic elements = parseXML(computedTargetPath + @"\searchIndex.xml");
            Dictionary<string, Dictionary<string, int>> precomputedIndex = new Dictionary<string, Dictionary<string, int>>();
            foreach (var keyword in elements.keyword)
            {
                Dictionary<string, int> imageCollection = new Dictionary<string, int>();
                foreach (var image in keyword.image)
                {
                    imageCollection.Add(image.id, Convert.ToInt32(image.score));
                }
                precomputedIndex.Add(keyword.value, imageCollection);
            }
            searchIndex = precomputedIndex;
        }

        public static void loadPrecomputedFileIndex()
        {
            dynamic elements = parseXML(computedTargetPath + @"\fileIndex.xml");
            Dictionary<string, string> precomputedFileIndex = new Dictionary<string, string>();
            foreach (var image in elements.image)
            {
                precomputedFileIndex.Add(image.id, image.path);
            }
            fileIndex = precomputedFileIndex;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            while (!System.IO.Directory.Exists(projectPath))
            {
                Console.WriteLine("Project Directory does not contain an neccessary directory/seems to be empty or cannot be found/accessed: '{0}'\n\nEnter new absolute Path to Project-Directory:\n(Project Directory must contain following Directories: 'xml' - XML-Metadatas,'img' - Images and 'descvis' - Featurepoints)\t", projectPath);
                string newProjectPath = Console.ReadLine();
                //xmlDirectory = newXmlPath;    //not neccessary
                Console.WriteLine("- Project-Path set to '{0}'", newProjectPath);
                projectPath = newProjectPath;
            }
            computedTargetPath = projectPath + @"\computed\";

            if (!System.IO.Directory.Exists(computedTargetPath))
            {
                Console.WriteLine("### First Start of Application - Initiating Creation of Index and FeaturepointsCollection ####");
                Console.WriteLine("\n\n\tPress Enter to Start (this can take a while!)");
                Console.ReadLine();
                initiateComputation();
            }
            else if (System.IO.File.Exists(computedTargetPath + @"\features.xml") && System.IO.File.Exists(computedTargetPath + @"\searchIndex.xml"))
            {
                Console.WriteLine(computedTargetPath);
                //computedPath Directory exists with computed files 
                Console.WriteLine("### Computed XML-Files (Index & FeaturePointCollection) already exist!\n");
                string option = "";
                while (!(option == "yes" || option == "no"))
                {
                    Console.WriteLine("\tDo you want to update Index and Feature Points? [yes/no]");
                    Console.WriteLine("Hint: Recalculating Index and Feature Points may take a long time!\n");
                    option = Console.ReadLine();
                }
                if (option == "yes")
                {
                    initiateComputation();
                }
                else
                {
                    //<string, Dictionary<string, int>>
                    loadPrecomputedSearchIndex();
                    loadPrecomputedFileIndex();
                }


            }

            //Dictionary<string, double> resultDistances = computeDistance("REAL ID HERE", picFeatures);

            ////startSimilaritySearch(resultDistances);

            //List<string> test = getImgPathsForImageSearch(resultDistances);

            //for (int i = 0; i < test.Count; i++)
            //{
            //    Console.WriteLine(test[i]);
            //}

            Console.WriteLine("Press Enter to continue:");
            Console.ReadLine();
            //printIndex();
            
            Application.Run(new frm_main());

        }
    }
}
