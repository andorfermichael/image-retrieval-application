//using Accord.Imaging;
//using Accord.MachineLearning;
//using Accord.Math;
//using Lucene.Net.Analysis.Standard;
//using Lucene.Net.Documents;
//using Lucene.Net.Index;
//using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
//using FSDirectory = Lucene.Net.Store.FSDirectory;
//using Version = Lucene.Net.Util.Version;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

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
        public static string imgDirectory = @"C:\Users\Nico\Downloads\img";
        public static string xmlDirectory = @"C:\Users\Nico\Downloads\xml";
        public static List<String> stopWords = new List<String> { "able", "about", "above", "abroad", "according", "accordingly", "across", "actually", "adj", "after", "again", "afterwards", "against", "ago", "ahead", "ain't", "all", "allow", "allows", "almost", "alone", "along", "alongside", "already", "also", "although", "always", "am", "amid", "amidst", "among", "amongst", "an", "and", "another", "any", "anybody", "anyhow", "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "appreciate", "appropriate", "are", "aren't", "around", "as", "a's", "aside", "ask", "asking", "associated", "at", "available", "away", "awfully", "back", "backward", "backwards", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "begin", "behind", "being", "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "both", "brief", "but", "by", "came", "can", "cannot", "cant", "can't", "caption", "cause", "causes", "certain", "certainly", "changes", "clearly", "c'mon", "co", "co.", "com", "come", "comes", "concerning", "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could", "couldn't", "course", "c's", "currently", "dare", "daren't", "definitely", "described", "despite", "did", "didn't", "different", "directly", "do", "does", "doesn't", "doing", "done", "don't", "down", "downwards", "during", "each", "edu", "eg", "eight", "eighty", "either", "else", "elsewhere", "end", "ending", "enough", "entirely", "especially", "et", "etc", "even", "ever", "evermore", "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except", "fairly", "far", "farther", "few", "fewer", "fifth", "first", "five", "followed", "following", "follows", "for", "forever", "former", "formerly", "forth", "forward", "found", "four", "from", "further", "furthermore", "get", "gets", "getting", "given", "gives", "go", "goes", "going", "gone", "got", "gotten", "greetings", "had", "hadn't", "half", "happens", "hardly", "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll", "hello", "help", "hence", "her", "here", "hereafter", "hereby", "herein", "here's", "hereupon", "hers", "herself", "he's", "hi", "him", "himself", "his", "hither", "hopefully", "how", "howbeit", "however", "hundred", "i'd", "ie", "if", "ignored", "i'll", "i'm", "immediate", "in", "inasmuch", "inc", "inc.", "indeed", "indicate", "indicated", "indicates", "inner", "inside", "insofar", "instead", "into", "inward", "is", "isn't", "it", "it'd", "it'll", "its", "it's", "itself", "i've", "just", "k", "keep", "keeps", "kept", "know", "known", "knows", "last", "lately", "later", "latter", "latterly", "least", "less", "lest", "let", "let's", "like", "liked", "likely", "likewise", "little", "look", "looking", "looks", "low", "lower", "ltd", "made", "mainly", "make", "makes", "many", "may", "maybe", "mayn't", "me", "mean", "meantime", "meanwhile", "merely", "might", "mightn't", "mine", "minus", "miss", "more", "moreover", "most", "mostly", "mr", "mrs", "much", "must", "mustn't", "my", "myself", "name", "namely", "nd", "near", "nearly", "necessary", "need", "needn't", "needs", "neither", "never", "neverf", "neverless", "nevertheless", "new", "next", "nine", "ninety", "no", "nobody", "non", "none", "nonetheless", "noone", "no-one", "nor", "normally", "not", "nothing", "notwithstanding", "novel", "now", "nowhere", "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "one's", "only", "onto", "opposite", "or", "other", "others", "otherwise", "ought", "oughtn't", "our", "ours", "ourselves", "out", "outside", "over", "overall", "own", "particular", "particularly", "past", "per", "perhaps", "placed", "please", "plus", "possible", "presumably", "probably", "provided", "provides", "que", "quite", "qv", "rather", "rd", "re", "really", "reasonably", "recent", "recently", "regarding", "regardless", "regards", "relatively", "respectively", "right", "round", "said", "same", "saw", "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed", "seeming", "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "shall", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "since", "six", "so", "some", "somebody", "someday", "somehow", "someone", "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "specified", "specify", "specifying", "still", "sub", "such", "sup", "sure", "take", "taken", "taking", "tell", "tends", "th", "than", "thank", "thanks", "thanx", "that", "that'll", "thats", "that's", "that've", "the", "their", "theirs", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "there'd", "therefore", "therein", "there'll", "there're", "theres", "there's", "thereupon", "there've", "these", "they", "they'd", "they'll", "they're", "they've", "thing", "things", "think", "third", "thirty", "this", "thorough", "thoroughly", "those", "though", "three", "through", "throughout", "thru", "thus", "till", "to", "together", "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying", "t's", "twice", "two", "un", "under", "underneath", "undoing", "unfortunately", "unless", "unlike", "unlikely", "until", "unto", "up", "upon", "upwards", "us", "use", "used", "useful", "uses", "using", "usually", "v", "value", "various", "versus", "very", "via", "viz", "vs", "want", "wants", "was", "wasn't", "way", "we", "we'd", "welcome", "well", "we'll", "went", "were", "we're", "weren't", "we've", "what", "whatever", "what'll", "what's", "what've", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "where's", "whereupon", "wherever", "whether", "which", "whichever", "while", "whilst", "whither", "who", "who'd", "whoever", "whole", "who'll", "whom", "whomever", "who's", "whose", "why", "will", "willing", "wish", "with", "within", "without", "wonder", "won't", "would", "wouldn't", "yes", "yet", "you", "you'd", "you'll", "your", "you're", "yours", "yourself", "yourselves", "you've", "zero" };
        public static List<String> propertiesToIndex = new List<string> { "date_taken", "description", "tags", "title", "username" };
        public static Dictionary<string, int> weightOfPorperties = new Dictionary<string, int>() {
            {"date_taken",1 },
            {"description",2 },
            {"tags",5 },
            {"title",10 },
            {"username",4 }
        };
        public static Dictionary<string, Dictionary<dynamic, int>> searchIndex = new Dictionary<string, Dictionary<dynamic, int>>();
        public static Dictionary<string, string> fileIndex = new Dictionary<string, string>();
        //                          /GLOBAL VARIABLES

        // TODO: In production replace switch from solutionDirectory to applicationDirectory
        //string applicationDirectory = Path.GetDirectoryName("../../" + Application.ExecutablePath);
        static string solutionDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));

        public static List<string> startTagSearch(string searchValue) {
            // TODO: Replace with real function code
            Debug.WriteLine("Search Value: " + searchValue.ToLower());
            return getImgPaths(searchFor(searchValue.ToLower()));
        }

        public static void startQueryByExampleSearch(string imageLocation)
        {
            // TODO: Replace with real function code
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

        private static void addItemToIndex(dynamic Item)
        {
            //  { "date_taken", "description", "tags", "title", "username" }
            foreach (string word in splitIntoWords(Item.date_taken))
                addWordToIndex(Item, word.ToLower(), weightOfPorperties["date_taken"]);
            foreach (string word in splitIntoWords(Item.description))
                addWordToIndex(Item, word.ToLower(), weightOfPorperties["description"]);
            foreach (string word in splitIntoWords(Item.tags))
                addWordToIndex(Item, word.ToLower(), weightOfPorperties["tags"]);
            foreach (string word in splitIntoWords(Item.title))
                addWordToIndex(Item, word.ToLower(), weightOfPorperties["title"]);
            foreach (string word in splitIntoWords(Item.username))
                addWordToIndex(Item, word.ToLower(), weightOfPorperties["username"]);
        }

        private static List<String> splitIntoWords(string completeString)
        {
            List<String> words = Regex.Matches(completeString, "\\w+")
              .OfType<Match>()
              .Select(m => m.Value)
              .ToList();
            return words;
        }

        public static void addWordToIndex(dynamic Item, string Property, int Weight)
        {
            //{ "date_taken", "description", "tags", "title", "username" };
            if (!(stopWords.Contains(Property)) && (Property.Length >= 3))
            {   //check if it is a stopWord, if not, add it to searchIndex
                if (!(searchIndex.ContainsKey(Property)))
                {   //check if Key exists already in Dictionary - if not:
                    searchIndex.Add(Property, new Dictionary<dynamic, int> { { Item, Weight } });
                    //Console.WriteLine("# Adding Key '{0}' with Item '{1}'#\n", word, Item.id);
                    Console.Write(".");
                }
                else
                {   //key exists already
                    //Console.WriteLine("## Key '{0}' already exists, adding Item '{1}' to set#\n", word, Item.id);
                    if (!(searchIndex[Property].ContainsKey(Item)))
                    {    //check if Item has already been added to indexed word/property - if not add it with current score
                        searchIndex[Property].Add(Item, Weight);
                    }
                    else
                    {
                        //item is already in list of indexed word/property, add scorepoints
                        //Console.WriteLine("Current Score of word " + Property + ": " + searchIndex[Property][Item]);
                        searchIndex[Property][Item] += Weight;
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
            createFileIndex(filepath, elements);
            return elements;
        }

        private static void createFileIndex(string directory, dynamic elements)
        {
            directory = directory.Replace(@"\xml\", @"\img\").Substring(0, directory.Length - 4) + @"\";
            foreach (dynamic item in elements.photo)
            {
                fileIndex.Add(item.id, directory + item.id + ".jpg");
                //Console.WriteLine(directory + item.id + ".jpg");
            }
        }

        private static List<KeyValuePair<dynamic,int>> searchFor(string searchString)
        {
            Dictionary<dynamic, int> results = new Dictionary<dynamic, int>();
            Dictionary<dynamic, int> resultsSingleQuery;

            foreach (string word in splitIntoWords(searchString))
            {
                resultsSingleQuery = new Dictionary<dynamic, int>();
                try
                {
                    foreach (KeyValuePair<dynamic, int> entry in searchIndex[word])
                    {
                        resultsSingleQuery.Add(entry.Key, entry.Value);
                    }
                    if (results.Count == 0)
                    {
                        foreach (KeyValuePair<dynamic, int> entry in resultsSingleQuery)
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
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("# Search for keyword '{0}' threw 0 results!",word);
                }
                catch (Exception e)
                {
                    Console.WriteLine("# Error! Following Error ocurred: " + e.Message);
                }
               
            }

            return sortResultsByScore(results);
        }

        private static List<string> getImgPaths(List<KeyValuePair<dynamic,int>> results)
        {
            List<string> matchingImages = new List<string>();
            foreach (KeyValuePair<dynamic, int> item in results)
                matchingImages.Add(fileIndex[item.Key.id]);
            return matchingImages;
        }

        private static List<KeyValuePair<dynamic, int>> sortResultsByScore(Dictionary<dynamic, int> results)
        {
            List<KeyValuePair<dynamic, int>> sortedResults = results.ToList();

            sortedResults.Sort(
                delegate (KeyValuePair<dynamic, int> pair1,
                KeyValuePair<dynamic, int> pair2)
                {
                    return pair2.Value.CompareTo(pair1.Value);
                }
            );

            return sortedResults;
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




        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine("### Grabbing IMG and XML-Files ###\n");
            List<String> imgSubDirectories = getSubDirectories(imgDirectory);
            List<String> xmlFiles = genXmlFilepaths(imgSubDirectories);

            while (xmlFiles.Count == 0)
            {
                try
                {
                    throw new FileNotFoundException();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Fatal! XML-Direcotry seems to be empty or cannot be found/accessed: '{0}'\n{1}\n\nEnter new absolute Path to XML-Directory:\t", xmlDirectory, e.Message);
                    string newXmlPath = Console.ReadLine();
                    //xmlDirectory = newXmlPath;    //not neccessary
                    Console.WriteLine("- XML-Path set to '{0}'\n\nPlease enter a new absolute path to IMG-Directory:\t", newXmlPath);
                    string newImgPath = Console.ReadLine();
                    Console.WriteLine("\nTrying to fetch Data with new paths...");
                    imgSubDirectories = getSubDirectories(newImgPath);
                    xmlFiles = genXmlFilepaths(imgSubDirectories);
                }

            }
            Console.WriteLine("### Parsing XML-File(s) ###\n");

            Console.WriteLine("### Creating Index (this can take a while) ####");
            foreach (string file in xmlFiles)
            {
                dynamic elements = parseXML(file);
                foreach (var photo in elements.photo)
                {
                    addItemToIndex(photo);
                }
            }
            Console.WriteLine("\n\n### Index creation successful! ###\n\n");
            Console.WriteLine("Press Enter to continue:");
            Console.ReadLine();
            //ImageFeatures ();

            //printIndex();

            Application.Run(new frm_main());

        }
    }
}
