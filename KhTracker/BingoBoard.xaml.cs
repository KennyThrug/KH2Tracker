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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
        struct BingoOption {
            public string tag;
            public string text;
            public bool isHintable; //Tells whether or not the Option is Hintable
            public bool isClicked;
            public System.Windows.Controls.Button button;
        }
        BingoOption[] options;
        Random rand = new Random(); //Seeded Random number bullshit

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

        //given the fileName, will open the zip, grab the spoilerlog, and use it
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
        //Helper Function that turns a string into an world enum
        world convertStringToWorld(string world)
        {
            //TODO this function
            return 0;
        }
        //Makes Reports report more hints for the bingoboard
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
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\"')
                {
                    if (numFirstQuote <= 0)
                    {
                        return res;
                    }
                    else
                    {
                        numFirstQuote--;
                    }
                }
                else if (numFirstQuote == 0)
                {
                    res += line[i];
                }
            }
            return "";
        }

        BingoOption easyMakeBingo(string text, string tag, bool isHintable)
        {
            BingoOption option = new BingoOption();
            option.text = text;
            option.tag = tag;
            option.isHintable = isHintable;
            option.isClicked = false;
            return option;
        }

        bool checkForTag(string tag,int numCell)
        {
            for(int i = 0; i < numCell; i++)
            {
                if (options[i].tag == tag || tag == "Failed")
                {
                    return true; //Means that the tag exists, reroll cell
                }
            }
            return false; //Means that tag doesn't exist, move on
        }
        BingoOption makeEasyCell(int numCell)
        {
            BingoOption option = easyMakeBingo("Placeholder",numCell.ToString(),false);
            switch (rand.Next(0,9))
            {
                case (0):
                    //Get A level 1 spell
                    break;
                case (1):
                    //Get a level 1 movement ability
                    break;
                case (2):
                    option = easyMakeBingo("Get to level 25", "Level", false);
                    break;
                case (3):
                    option = easyMakeBingo("Save Piglet from the Blustery Winds", "HundredAcre", false);
                    break;
                case (4):
                    option = easyMakeBingo("Get " + rand.Next(3, 5) + " Secret Ansem Reports", "Ansem Reports", false);
                    break;
                case (5):
                    option = easyMakeBingo("Get " + rand.Next(5, 9) + " Keychains", "Keychains", false);
                    break;
                case (6):
                    //Fight a non superboss once
                    break;
                case (7):
                    //Get all Puzzle Pieces from a world
                    break;
                case (8):
                    return makeNormalCell(numCell);
                case (9):
                    return makeStupidCell(numCell);
            }
            try
            {
                if (checkForTag(option.tag, numCell))
                    return makeEasyCell(numCell);
            }
            catch(Exception e)
            {
                return makeEasyCell(numCell);
            }
            return option;
        }
        BingoOption makeNormalCell(int numCell)
        {
            BingoOption option = easyMakeBingo("Placeholder", numCell.ToString(), false);
            switch (rand.Next(0, 14))
            {
                case (0):
                    //Get an Item, ability, or keyblade
                    break;
                case (1):
                    //Get a level 2 spell
                    break;
                case (2):
                    //Defeat a boss in a superboss location
                    break;
                case (3):
                    //Get a level 3 movement ability
                    break;
                case (4):
                    //Beat a specific world
                    break;
                case (5):
                    easyMakeBingo("Get to level 50", "Level", false);
                    break;
                case (6):
                    //Get a drive form
                    break;
                case (7):
                    //Get a Summon
                    break;
                case (8):
                    easyMakeBingo("Get " + rand.Next(6, 9) + " Secret Ansem Reports", "Ansem Reports", false);
                    break;
                case (9):
                    easyMakeBingo("Get " + rand.Next(10, 19) + " Keychains", "Keychains", false);
                    break;
                case (10):
                    //Fight a non-superboss 1-3 times
                    break;
                case (11):
                    //Fight a superboss once
                    break;
                case (12):
                    //Finish a small puzzle
                    break;
                case (13):
                    return makeEasyCell(numCell);
                case (14):
                    return makeHardCell(numCell);
            }

            if (checkForTag(option.tag, numCell))
                return makeNormalCell(numCell);
            return option;
        }
        BingoOption makeHardCell(int numCell)
        {
            BingoOption option = easyMakeBingo("Placeholder", numCell.ToString(), false);
            switch (rand.Next(0, 10))
            {
                case (0):
                    //Get a level 3 spell
                    break;
                case (1):
                    //Get a Max Level movement ability
                    break;
                case (2):
                    easyMakeBingo("Finish Hundred Acre Woods", "HundredAcre", false);
                    break;
                case (3):
                    easyMakeBingo("Beat Atlantica", "Atlantica", false);
                    break;
                case (4):
                    easyMakeBingo("Get to level 99", "Levels", false);
                    break;
                case (5):
                    easyMakeBingo("Get all Drive forms", "Drive", false);
                    break;
                case (6):
                    easyMakeBingo("Get all Summons", "Summons", false);
                    break;
                case (7):
                    easyMakeBingo("Get " + rand.Next(10, 13) + " Secret Ansem Reports", "Ansem Reports", false);
                    break;
                case (8):
                    easyMakeBingo("Get " + rand.Next(20, 29) + "Keychains", "Keychains", false);
                    break;
                case (9):
                    //Fight a superboss 1-3 times
                    break;
                case (10):
                    //Fight a non-superboss 1-10 times
                    break;
                case (11):
                    //Finish a large puzzle
                    break;
                case (12):
                    return makeNormalCell(numCell);
            }

            if (checkForTag(option.tag, numCell))
                return makeHardCell(numCell);
            return option;
        }
        BingoOption makeStupidCell(int numCell)
        {
            BingoOption option = easyMakeBingo("Placeholder", numCell.ToString(), false);
            switch (rand.Next(0, 11))
            {
                case (1):
                    option = easyMakeBingo("Destroy all Tents in Shang's Camp", "Camp", false);
                    break;
                case (2):
                    option = easyMakeBingo("Destroy all stalls in Agrabah's Bazzar", "Bazzar", false);
                    break;
                case (3):
                    option = easyMakeBingo("Dodge roll Sephiroth's reaction Command", "SepRC", false);
                    break;
                case (4):
                    option = easyMakeBingo("Fail Demyx's Minigame", "DemyxMini", false);
                    break;
                case (5):
                    option = easyMakeBingo("Watch Sora get Punched in the Face", "SoraPunch", false);
                    break;
                case (6):
                    option = easyMakeBingo("Get Saved By Mickey", "SavedMickey", false);
                    break;
                case (7):
                    option = easyMakeBingo("Get to the Top of Sunset Hill as Sora", "SunsetHill", false);
                    break;
                case (8):
                    option = easyMakeBingo("Get A high Score in a skateboard Minigame", "Skateboard", false);
                    break;
                case (9):
                    option = easyMakeBingo("Crash the Game", "Crash", false);
                    break;
                case (10):
                    option = easyMakeBingo("Perform Leon's Reaction command", "LeonRC", false);
                    break;
                case (11):
                    return makeEasyCell(numCell);
                case (12):
                    return makeHardCell(numCell);
            }

            if (checkForTag(option.tag, numCell))
                return makeStupidCell(numCell);
            return option;
        }

        //Creates a new Bingo Cell, the real magic of this class
        BingoOption createCell(int numCell, short difficulty)
        {
            BingoOption cell = new BingoOption();
            switch (difficulty){
                case (0):
                {
                    cell = makeEasyCell(numCell);
                    break;
                };
                case (1):
                {
                    cell = makeNormalCell(numCell);
                    break;
                };
                case (2):
                {
                    cell = makeHardCell(numCell);
                    break;
                };
            }
            return cell;
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
                    int seedNum = 1;
                    string seed = getValueFromLine(line, 1);
                    foreach(char character in seed)
                    {
                        seedNum += character;
                    }
                    rand = new Random(seedNum);
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

        //index, refers to the index of the cell, starting at top left, moving down, then wrapping around
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
                    short tempDif = 0;
                    if((difficulty == 0 && i <= 18) || 
                        (difficulty == 1 && i <= 4) || 
                        (difficulty == 2 && i < 0)) { tempDif = 0; }
                    else if((difficulty == 0 && i <= 25) ||
                        (difficulty == 1 && i <= 20) ||
                        (difficulty == 2 && i <=10)) { tempDif = 1; }
                    else { tempDif = 3; }

                    options[i] = createCell(i,tempDif);
                    addButton(options[i], i);
                }
            }
        }

    }
}
