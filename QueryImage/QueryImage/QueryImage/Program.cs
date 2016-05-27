using Accord.Imaging;
using Accord.MachineLearning;
using Accord.Math;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using Version = Lucene.Net.Util.Version;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
//using Accord.Math.Distances;

namespace QueryImage
{
    public static class MainClass
	{
		internal static readonly DirectoryInfo INDEX_DIR = new DirectoryInfo ("index");

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
		//                          /GLOBAL VARIABLESad
        
        public static void Main (string[] args)
		{
            //Index ();
            //Query ();
            //parseXML("ajanta_caves.xml");
            Console.WriteLine("### Grabbing IMG and XML-Files ###\n");
            List<String> imgSubDirectories = getSubDirectories(imgDirectory);
            List<String> xmlFiles = genXmlFilepaths(imgSubDirectories);

            while(xmlFiles.Count == 0) {
                try 
                {
                    throw new FileNotFoundException();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Fatal! XML-Direcotry seems to be empty or cannot be found/accessed: '{0}'\n{1}\n\nEnter new absolute Path to XML-Directory:\t", xmlDirectory,e.Message);
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
            dynamic elements = parseXML(xmlFiles[0]);
            //Console.WriteLine(photos);

            Console.WriteLine("### Creating Index (this can take a while) ####");
            foreach (var photo in elements.photo)
            {
                addItemToIndex(photo);
            }

            Console.WriteLine("\n\n### Index creation successful! ###\n\n");
            Console.WriteLine("Press Enter to print Index [humanized]:");
            Console.ReadLine();
            //ImageFeatures ();

            foreach (KeyValuePair<string, Dictionary<dynamic, int>> entry in searchIndex)
            {
                if (entry.Value.Count() == 1)
                {
                    Console.WriteLine("Word {0} is part of Photo: {1}.jpg \tScore: {2}\n", entry.Key, entry.Value.First().Key.id, entry.Value.First().Value);
                }
                else
                {
                    Console.WriteLine("Word {0} is part of following Photos: \n", entry.Key);
                    foreach (dynamic subitem in entry.Value)
                    {
                        Console.WriteLine("\t{0}\tScore: {1}\n", subitem.Key.id, subitem.Value);
                    }
                }
            }
           
            Console.ReadLine();
		}

        private static void addItemToIndex(dynamic Item)
        {
            //Console.WriteLine("Adding Item with id:'{0}' to Index", Item.id);

            // Unfortunately iterating over Properties doesn't work with a dynamic object!
            // Instead we are using hardcoded properties List - see globals "propertiesToIndex"

            //foreach (var prop in Item.GetType().GetProperties())
            //{              
            //    Console.WriteLine(prop.GetValue(Item, null));
            //    if (!(stopWords.Find(prop.GetValue())))
            //    {
            //        //check if it is a stopWord, if not, add it to searchIndex
            //        searchIndex.Add(prop.GetValue(),Item);
            //    }
            //}

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
        public static Object GetPropValue(this Object obj, String name)
        {
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        public static T GetPropValue<T>(this Object obj, String name)
        {
            Object retval = GetPropValue(obj, name);
            if (retval == null) { return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }

        private static List<String> splitIntoWords(string completeString)
        {
            List<String> words = Regex.Matches(completeString, "\\w+")
              .OfType<Match>()
              .Select(m => m.Value)
              .ToList();
            return words;
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

        private static dynamic parseXML(string filepath)
        {
            //            string xml = @"<Students>
            //                <Student ID=""100"">
            //                    <Name>Arul</Name>
            //                    <Mark>90</Mark>
            //                </Student>
            //                <Student>
            //                    <Name>Arul2</Name>
            //                    <Mark>80</Mark>
            //                </Student>
            //            </Students>";

            Console.WriteLine("parsing XML: " + filepath);

            string xml = File.ReadAllText(filepath);
            dynamic elements = DynamicXml.Parse(xml);
            return elements;
        }

		private static void Index ()
		{
			try {
				using (var writer = new IndexWriter (FSDirectory.Open (INDEX_DIR), new StandardAnalyzer (Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED)) {
					Document doc1 = new Document ();
					doc1.Add (new Field ("name", "doc 1", Field.Store.YES, Field.Index.NO));
					doc1.Add (new Field ("content", "abc xyz", Field.Store.YES, Field.Index.ANALYZED));
					writer.AddDocument (doc1);

					Document doc2 = new Document ();
					doc2.Add (new Field ("name", "doc 2", Field.Store.YES, Field.Index.NO));
					doc2.Add (new Field ("content", "abc defg defg defg", Field.Store.YES, Field.Index.ANALYZED));
					writer.AddDocument (doc2);

					Document doc3 = new Document ();
					doc3.Add (new Field ("name", "doc 3", Field.Store.YES, Field.Index.NO));
					doc3.Add (new Field ("content", "qwerty defg defg", Field.Store.YES, Field.Index.ANALYZED));
					writer.AddDocument (doc3);

					Console.Out.WriteLine ("Optimizing...");
					writer.Optimize ();
					writer.Commit ();
					writer.Dispose ();

				}
			} catch (IOException e) {
				Console.Out.WriteLine (" caught a " + e.GetType () + "\n with message: " + e.Message);
			}
		}


		private static void Query ()
		{
			try {
				IndexReader reader = IndexReader.Open (FSDirectory.Open (INDEX_DIR), true);
				Console.Out.WriteLine ("Number of indexed docs: " + reader.NumDocs ());

				IndexSearcher searcher = new IndexSearcher (FSDirectory.Open (INDEX_DIR));
				Term searchTerm = new Term ("content", "defg");
				TermQuery query = new TermQuery (searchTerm);

				TopScoreDocCollector topDocColl = TopScoreDocCollector.Create (10, true);
				searcher.Search (query, topDocColl);
				TopDocs topDocs = topDocColl.TopDocs ();
				Console.Out.WriteLine ("Number of hits: " + topDocs.TotalHits);
				ScoreDoc[] docarray = topDocs.ScoreDocs;

				for (int i = 0; i < docarray.Length; i++) {
					Console.Out.WriteLine ((i + 1) + ". " + (searcher.Doc (docarray [i].Doc)).GetField ("name").StringValue);
				}

			} catch (IOException e) {
				Console.Out.WriteLine (" caught a " + e.GetType () + "\n with message: " + e.Message);
			}

		}


		private static void ImageFeatures ()
		{
			Dictionary<string, Bitmap> testImages = new Dictionary<string, Bitmap>();
            
			testImages.Add("img_acropolis", (Bitmap)Bitmap.FromFile("test_imgs/acropolis_athens.jpg"));
			testImages.Add("img_cathedral", (Bitmap)Bitmap.FromFile("test_imgs/amiens_cathedral.jpg"));
			testImages.Add("img_bigben", (Bitmap)Bitmap.FromFile("test_imgs/big_ben.jpg"));

			int numberOfWords = 6; // number of cluster centers: typically >>100

			// Create a Binary-Split clustering algorithm
			BinarySplit binarySplit = new BinarySplit(numberOfWords);

			IBagOfWords<Bitmap> bow;
			// Create bag-of-words (BoW) with the given algorithm
			BagOfVisualWords surfBow = new BagOfVisualWords(binarySplit);

			// Compute the BoW codebook using training images only
			Bitmap[] bmps = new Bitmap[testImages.Count];
			testImages.Values.CopyTo(bmps, 0);
			surfBow.Compute(bmps);
			bow = surfBow; 	// this model needs to be saved once it is calculated: only compute it once to calculate features 
							// from the collection as well as for new queries.
							// THE SAME TRAINED MODEL MUST BE USED TO GET THE SAME FEATURES!!!

			Dictionary<string, double[]> testImageFeatures = new Dictionary<string, double[]>();

			// Extract features for all images
			foreach (string imagename in testImages.Keys)
			{
				double[] featureVector = bow.GetFeatureVector(testImages[imagename]);
				testImageFeatures.Add (imagename, featureVector);
				Console.Out.WriteLine (imagename + " features: " + featureVector.ToString(DefaultArrayFormatProvider.InvariantCulture));
			}
			// Calculate Image Similarities
			string[] imagenames = new string[testImageFeatures.Keys.Count];
			testImageFeatures.Keys.CopyTo(imagenames, 0);
			for (int i = 0; i < imagenames.Length; i++) {
				for (int j = i + 1; j < imagenames.Length; j++) {
					double dist = Distance.Cosine (testImageFeatures[imagenames[i]], testImageFeatures[imagenames[j]]);
					Console.Out.WriteLine (imagenames[i] + " <-> " + imagenames[j] + " distance: " + dist.ToString());
				}
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
                Console.WriteLine("Warning! Unauthorized Access Exception was thrown! Access to {0} was denied!",path);
                return new List<string>();
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal! Reading Directory '{0}' resultet in following Error: {1}", path, e.Message);
                return new List<string>();
            }
        }
    }
    public class Map<T1, T2>
    {
        private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

        public Map()
        {
            this.Forward = new Indexer<T1, T2>(_forward);
            this.Reverse = new Indexer<T2, T1>(_reverse);
        }

        public Map(int score, dynamic item)
        {
            this.Forward = new Indexer<T1, T2>(_forward);
            this.Reverse = new Indexer<T2, T1>(_reverse);
        }

        public class Indexer<T3, T4>
        {
            private Dictionary<T3, T4> _dictionary;
            public Indexer(Dictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }
            public T4 this[T3 index]
            {
                get {
                        try 
                        {
                            return _dictionary[index];
                        }
                        catch (System.Collections.Generic.KeyNotFoundException)
                        {
                            return default(T4);
                        }
                    }
                set { _dictionary[index] = value; }
            }
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public int Count()
        {
            if (_forward.Count == _reverse.Count)
                return _forward.Count;
            else
                throw new InvalidDataException();
        }

        public Dictionary<T1, T2> getForward()
        {
            return (_forward);
        }

        public Dictionary<T2, T1> getReverse()
        {
            return (_reverse);
        }

        public Indexer<T1, T2> Forward { get; private set; }
        public Indexer<T2, T1> Reverse { get; private set; }
    }
}
