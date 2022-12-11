using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class BingoBoard : Window
    {
        short difficulty; // 0 : Easy, 1 : Normal, 2 : Hard
        //Easy : 0 Hard cells, 8 Normal, 17 Easy
        //Normal : 6 Hard Cells, 15 Normal, 4 Easy
        //Hard : 15 Hard Cells, 10 Normal, 0 Easy
        //BingoOption is the things that are going on the board
        struct BingoOption{
            public string tag;
            public string text;
            public bool isHintable; //Tells whether or not the Option is Hintable
            public bool isClicked;
            public System.Windows.Controls.Button button;
        }
        BingoOption[] options;
        string seedName;

        //lists of all items and bosses in the seed
        struct Items
        {
            public world worldName;
            public string itemName;
        }
        List<Items> listOfItems = new List<Items>();
        List<Items> listOfBosses = new List<Items>();
        //A list of all hints added via Bingo, using world enum
        int[] bingoReports = new int[18];

        //Dictionaries :
        //enumeration of all the worlds, for the purposese of arrays
        enum world
        {
            SoraHeart, Drive, STT,
            TwilightTown, RadiantGarden, LOD,
            BeastCastle, Olympus, DisneyCastle, PortRoyal, Agrabah,
            HalloweenTown, SpaceParanoids, PrideRock, TWTNW,
            HundredAcre, Atlantica, Garden, Puzzles
        }
        //dictionary of every item in the game
        List<string> dictionaryOfItems;
        //dictionary of every boss in the game and which world they are in
        List<string>[] dictionaryOfBosses = new List<string>[15] {
            new List<string> { "" },//Sora's Heart (no bosses)
            new List<string> { "" },//Drive (no Bosses)
            new List<string> { //STT
            "Axel I","Axel II","Roxas (Data)","Twilight Thorn"},
            new List<string> { //Twilight Town
            "Axel (Data)"},
            new List<string> { //Radiant Garden/Hollow Bastion
            "Demyx","Demyx (Data)","Sephiroth"},
            new List<string> { //Land of Dragons
            "Shan-Yu","Storm Rider","Xigbar (Data)",},
            new List<string> { //Beast's Castle
            "Dark Thorn","Shadow Stalker","The Beast","Thresholder","Xaldin","Xaldin (Data)"},
            new List<string> { //Olympus
            "Blizzard Lord (Cups)", "Cerberus", "Cerberus (Cups)","Cloud","Cloud (1)","Cloud (2)","Hades Cups","Hades II","Hades II (1)","Hercules","Hydra","Leon","Leon (1)","Leon (2)","Leon (3)","Pete Cups","Pete OC II","Tifa","Tifa (1)","Tifa (2)","Volcano Lord (Cups)","Yuffie","Yuffie (1)","Yuffie (2)","Yuffie (3)","Zexion","Zexion (Data)"},
            new List<string> { //Disney Castle
            "Past Pete","Terra","Marluxia","Marluxia (Data)"},
            new List<string> { //Port Royal
            "Barbossa", "Grim Reaper I","Grim Reaper II","Luxord (Data)"},
            new List<string> { //Agrabah
            "Blizzard Lord","Volcano Lord","Lexaus","Lexaus (Data)"},
            new List<string> { //Halloween Town
            "Oogie Boogie","Prison Keeper","The Experiment","Vexen","Vexen (Data)"},
            new List<string> { //Space Paranoids
            "Hostile Program","Larxene","Larxene (Data)","Sark"},
            new List<string> { //Pride Rock
            "Groundshaker","Scar","Saix (Data)"},
            new List<string> { //TWTNW
            "Armor Xemnas I", "Armor Xemnas II", "Final Xemnas","Final Xemnas (Data)","Luxord","Roxas","Saix","Xemnas","Xemnas (Data)","Xigbar"}
        };

        public BingoBoard(string fileName)
        {
            InitializeComponent();
            goThroughHints(fileName);
            generateBingoButtons();
        }


        private void goThroughHints(string fileName)
        {
            ZipArchiveEntry spoiler = null;
            using (ZipArchive archive = ZipFile.OpenRead(fileName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.Equals("spoilerlog.html"))
                    {
                        spoiler = entry;
                    }
                }
                var spoilerFile = new HtmlAgilityPack.HtmlDocument();
                spoilerFile.Load(spoiler.Open());
                HtmlNode[] nodes = spoilerFile.DocumentNode.SelectNodes("//script").ToArray();
                var spoilerText = nodes[0].InnerHtml;
                string[] lines = spoilerText.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                    );
                parseData(lines);
                //TODO: Go through all items, put them into arrays
            }
            //Go through all worlds and add to temp tuple, then replace original
            List<Tuple<string, string, int>> tempReportInformation = new List<Tuple<string, string, int>>();
            for (int i = 0; i < 13; i++) {
                tempReportInformation.Add(new Tuple<string, string, int>(
                MainWindow.data.reportInformation[i].Item1,
                MainWindow.data.reportInformation[i].Item2,
                MainWindow.data.reportInformation[i].Item3 +
                getHintsPerWorld(MainWindow.data.reportInformation[i].Item2)));
            };
            MainWindow.data.reportInformation.Clear();
            MainWindow.data.reportInformation = tempReportInformation;
        }
        
        world convertStringToWorld(string world)
        {
            //TODO this function
            return 0;
        }

        void addHintsOnWorld(string world, int numHints)
        {
            addHintsOnWorld(convertStringToWorld(world), numHints);
        }
        void addHintsOnWorld(world worldID, int numHints)
        {
            bingoReports[(int)worldID] += numHints;
        }

        string getValueFromLine(string line, int numFirstQuote)
        {
            string res = "";
            for(int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\"')
                {
                    if(numFirstQuote <= 0)
                    {
                        return res;
                    }
                    else
                    {
                        numFirstQuote--;
                    }
                }
                else if(numFirstQuote == 0)
                {
                    res += line[i];
                }
            }
            return "";
        }
        
        private void parseBossData(string line)
        {
            string tempString = "";
            world worldNum = world.SoraHeart;
            bool innerQuote = false;
            short isOriginal = 0; // 0 = nothing, 1 = original, 2 = new
            //TODO parse this data
            foreach(char curChar in line) {
                if(curChar == '\"')
                {
                    if(innerQuote == false)
                    {
                        innerQuote = true;
                    }
                    else
                    {
                        //Handle a full word
                        if(tempString == "original")
                        {
                            isOriginal = 1;
                        }
                        else if(tempString == "new")
                        {
                            isOriginal = 2;
                        }
                        else if(isOriginal == 1)
                        {
                            worldNum = (world)findLocationOfBoss(tempString);
                            isOriginal = 0;
                        }
                        else if(isOriginal == 2)
                        {
                            Items tempItem = new Items();
                            tempItem.worldName = worldNum;
                            tempItem.itemName = tempString;
                            listOfBosses.Add(tempItem);
                            isOriginal = 0;
                        }
                        tempString = "";
                        innerQuote = false;
                    }
                }
                else if (innerQuote)
                {
                    tempString += curChar;
                }
            }
        }
        //Helper function that finds the location of the boss using the dictionaryOfBosses
        public int findLocationOfBoss(string bossName)
        {
            for(int i = 0; i< dictionaryOfBosses.Length; i++)
            {
                for(int j = 0; j < dictionaryOfBosses[i].Count; j++)
                {
                    if(bossName == dictionaryOfBosses[i][j])
                    {
                        return i;
                    }
                }
            }
            return 0; //No boss is found
        }
        public void parseData(string[] allLines)
        {
            world curWorld = world.SoraHeart;
            bool available = false;
            foreach (string line in allLines) {
                if (line.Contains("seed_name"))
                {
                    getValueFromLine(line, 1);
                }
                else if (line.Contains("boss_enemy_data"))
                {
                    parseBossData(line);
                }
                else if (line.Contains("Name") && available)
                {
                    string x = getValueFromLine(line,3);
                    if (x != "")
                    {
                        Items itemp = new Items();
                        itemp.worldName = curWorld;
                        itemp.itemName = x;
                        listOfItems.Add(itemp);
                    }
                        
                }
                else if (line.Contains('['))
                {
                    if (line.Contains("Slot") || line.Contains("Garden of Assemblage") ||
                        line.Contains("Critical Bonuses"))
                    {
                        available = false;
                    }
                    else if (line.Contains("TwilightTown"))
                    {
                        available = true;
                        curWorld = (world.TwilightTown);
                    }
                    else if (line.Contains("Olympus"))
                    {
                        available = true;
                        curWorld = (world.Olympus);
                    }
                    else if (line.Contains("Beast's Castle"))
                    {
                        available = true;
                        curWorld = (world.BeastCastle);
                    }
                    else if (line.Contains("Disney Castle"))
                    {
                        available = true;
                        curWorld = (world.DisneyCastle);
                    }
                    else if (line.Contains("Level"))
                    {
                        available = true;
                        curWorld = (world.SoraHeart);
                    }
                    else if (line.Contains("Hollow Bastion"))
                    {
                        available = true;
                        curWorld = (world.RadiantGarden);
                    }
                    else if (line.Contains("Pride Lands"))
                    {
                        available = true;
                        curWorld = (world.PrideRock);
                    }
                    else if (line.Contains("Land of Dragons"))
                    {
                        available = true;
                        curWorld = (world.LOD);
                    }
                    else if (line.Contains("Agrabah"))
                    {
                        available = true;
                        curWorld = (world.Agrabah);
                    }
                    else if (line.Contains("Halloween Town"))
                    {
                        available = true;
                        curWorld = (world.HalloweenTown);
                    }
                    else if (line.Contains("Simulated Twilight Town"))
                    {
                        available = true;
                        curWorld = (world.STT);
                    }
                    else if (line.Contains("Port Royal"))
                    {
                        available = true;
                        curWorld = (world.PortRoyal);
                    }
                    else if (line.Contains("Space Paranoids"))
                    {
                        available = true;
                        curWorld = (world.SpaceParanoids);
                    }
                    else if (line.Contains("The World That Never Was"))
                    {
                        available = true;
                        curWorld = (world.TWTNW);
                    }
                    else if (line.Contains("Form Levels") || line.Contains("SummonLevel"))
                    {
                        available = true;
                        curWorld = (world.Drive);
                    }
                    else if (line.Contains("Hundred Acre Wood"))
                    {
                        available = true;
                        curWorld = (world.HundredAcre);
                    }
                    else if (line.Contains("Atlantica"))
                    {
                        available = true;
                        curWorld = (world.Atlantica);
                    }
                    else if (line.Contains("Puzzle"))
                    {
                        available = true;
                        curWorld = (world.Puzzles);
                    }
                }
            }
        }

        int getHintsPerWorld(string worldName)
        {
            //Console.WriteLine(worldName);
            return 0;
        }

        //index, refers to the index of the cell, starting at top left, moving to the right, then wrapping around
        private void addButton(BingoOption bingoOption,int index)
        {
            int row = 0;
            int col = Math.DivRem(index,5,out row);
            bingoOption.button = new System.Windows.Controls.Button();
            bingoOption.button.Content = bingoOption.text;
            Grid.SetRow(bingoOption.button, row);
            Grid.SetColumn(bingoOption.button, col);
            BingoGrid.Children.Add(bingoOption.button);
        }
        private void generateBingoButtons()
        {
            options = new BingoOption[25];
            for(int i = 0; i < 25; i++)
            {
                if (i == 12) //Always is "Beat the Game"
                {
                    options[i] = new BingoOption();
                    options[i].text = "Beat The Game";
                    options[i].tag = "FinishGame";
                    options[i].isHintable = false;
                    options[i].isClicked = false;
                    addButton(options[i], i);

                }
                else
                {
                    options[i] = new BingoOption();
                    options[i].text = i.ToString();
                    options[i].isHintable = false;
                    options[i].isClicked = false;
                    addButton(options[i], i);
                }
            }
        }

    }
}
