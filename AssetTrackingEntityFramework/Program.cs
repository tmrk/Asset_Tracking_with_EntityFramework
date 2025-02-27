﻿using System.Globalization;

namespace AssetTrackingEntityFramework
{
    internal class Program
    {
        static MyDbContext Context = new MyDbContext();

        // --------------- SETTING UP SOME OPTIONS & VARIABLES ---------------

        public static bool filterTableTemp = false;

        public static string orderByTemp = "";

        public static string thenByTemp = "";

        static List<Asset> assets = new List<Asset>();

        static List<Asset> assetsTemp = new List<Asset>();

        static List<Column> columns = new List<Column>
        {
            new Column("Type", 10),
            new Column("Brand", 10),
            new Column("Model", 10),
            new Column("Office", 10),
            new Column("Purchase Date", 10),
            new Column("Price USD", 6),
            new Column("", 3, propertyName: "DefaultCurrency"),
            new Column("Local Price"),
            new Column("Currency")
        };

        static List<MenuFunction> mainMenu = new List<MenuFunction>
        {
            new MenuFunction("Show all assets unsorted", ()=> ShowState(mainMenu, filterTable: false, orderBy: "", thenBy: "")),
            new MenuFunction("Add New Asset", ()=> AddNew()),
            new MenuFunction("Sort by type of asset", ()=> ShowState(mainMenu, subheading: "Sorted by Type", filterTable: filterTableTemp, orderBy: "Type")),
            new MenuFunction("Sort by purchase date", ()=> ShowState(mainMenu, subheading: "Sorted by Purchase Date", filterTable: filterTableTemp, orderBy: "PurchaseDate")),
            new MenuFunction("Sort by Office then Purchase Date", ()=> ShowState(mainMenu, subheading: "Sorted by Office then Purchase Date", filterTable: filterTableTemp, orderBy: "Office", thenBy: "PurchaseDate")),
            new MenuFunction("Mark end of life assets", ()=> ShowState(mainMenu, filterTable: true, orderBy: orderByTemp, thenBy: thenByTemp, subheading: "Mark end of life assets | Red: within 3 months | Yellow: within 6 months")),
            new MenuFunction("Quit", ()=> Environment.Exit(0))
        };

        static void LoadDB()
        {
            Console.WriteLine("Loading database. Please wait...");
            assets = Context.Assets.ToList();
            if (assets.Count == 0) AddTestData();
            Console.Clear();
            Console.WriteLine("Database loaded.");
        }

        static void Main(string[] args)
        {
            Console.WindowHeight = 40;
            Console.Title = "Asset Tracking with EntityFramework";
            LoadDB();
            ShowState(mainMenu);
        }


        // --------------- PROGRAM METHODS ---------------

        static public void ShowState(List<MenuFunction> menu, int menuSelected = 1, string subheading = "", bool filterTable = false, string orderBy = "", string thenBy = "")
        {
            // Set or reset global options so that they stay the same between state changes
            filterTableTemp = filterTable;
            orderByTemp = orderBy;
            thenByTemp = thenBy;

            // Draw the current state onto the console
            PrintHeader(subheading: subheading);
            PrintAssets();
            ShowMenu(menu, menuSelected, subheading);
            SelectMenu(menu, menuSelected, subheading);
        }

        // Display a title at the top, incl. subtitle
        static void PrintHeader(string subheading = "")
        {
            Console.Clear();
            Panel("heading", "Asset Tracking", subheading: subheading, width: 80, tMargin: 1);
        }

        static void PrintAssets()
        {
            if (orderByTemp != "")
            {
                if (thenByTemp != "") assetsTemp = assets
                        .OrderBy(asset => asset.GetType().GetProperty(orderByTemp).GetValue(asset))
                        .ThenBy(asset => asset.GetType().GetProperty(thenByTemp).GetValue(asset))
                        .ToList();
                else assetsTemp = assets
                        .OrderBy(asset => asset.GetType().GetProperty(orderByTemp).GetValue(asset))
                        .ToList();
            }
            else assetsTemp = assets;
            Panel("table",
                cols: columns,
                colspan: 2,
                rows: assetsTemp,
                filterTable: filterTableTemp);
        }

        // Allows user to input a new asset
        static public void AddNew()
        {
            Console.Clear();
            Asset newAsset = new Asset("", "", "", "", DateTime.Now, 0);
            Console.CursorVisible = true;
            Panel("heading", "Add New Asset", width: 80, tMargin: 1, bMargin: 1);

            int hMargin = (Console.WindowWidth / 2) - (80 / 2);
            string input = "";
            int year, month, daysInMonth, price = 0;
            DateTime date, yearCheck = new DateTime();
            bool isNumber, isDate = false;

            // Type of asset
            Console.Write("\n".PadRight(hMargin) + "Enter the type of the asset (Phone, Computer, etc): ");
            do
            {
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                else newAsset.Type = input;
            }
            while (String.IsNullOrEmpty(input.Trim()));

            // Brand of asset
            Console.Write("\n".PadRight(hMargin) + "Enter brand (Apple, Samsung, etc): ");
            do
            {
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                else newAsset.Brand = input;
            }
            while (String.IsNullOrEmpty(input.Trim()));

            // Model of asset
            Console.Write("\n".PadRight(hMargin) + "Enter model (Galaxy S, iPhone XS, etc): ");
            do
            {
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                else newAsset.Model = input;
            }
            while (String.IsNullOrEmpty(input.Trim()));

            // Office
            Console.Write("\n".PadRight(hMargin) + "Enter office (Sweden, UK, etc): ");
            do
            {
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                else newAsset.Office = input;
            }
            while (String.IsNullOrEmpty(input.Trim()));

            // Purchase date
            // Add year
            Console.WriteLine("\n".PadRight(hMargin) + "Enter the date of purchase");
            do
            {
                Console.Write("".PadRight(hMargin + 2) + "Year: ");
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                isDate = DateTime.TryParse(string.Format("{0}/1/1", input), out yearCheck);
            }
            while (!isDate);
            isDate = false; // reset it because it's needed again below
            year = yearCheck.Year;

            // Add month
            do
            {
                Console.Write("".PadRight(hMargin + 2) + "Month (1-12): ");
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                int.TryParse(input, out month);
            }
            while (month < 1 || month > 12);
            
            // Add day
            do
            {
                daysInMonth = DateTime.DaysInMonth(year, month);
                Console.Write("".PadRight(hMargin + 2) + $"Day (1-{ daysInMonth }): ");
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                isDate = DateTime.TryParse(string.Format("{0}/{1}/{2}", year, month, input), out date);
            }
            while (!isDate);
            newAsset.PurchaseDate = date;

            // Price
            Console.Write("\n".PadRight(hMargin) + $"Enter price in { newAsset.Currency }: ");
            do
            {
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(mainMenu);
                isNumber = int.TryParse(input, out price);
            }
            while (!isNumber);
            newAsset.LocalPrice = Convert.ToDouble(price);

            Context.Assets.Add(newAsset);
            Context.SaveChanges();
            assets = Context.Assets.ToList();
            ShowState(mainMenu);
        }

        // Returns a string from an Asset that spaces the asset's properties based on the passed List that defines column names and column widths
        static string PrintAssetByColumns(Asset asset, List<Column> cols, int colspan = 1)
        {
            string row = "";
            for (int i = 0; i < cols.Count; i++)
            {
                Column column = cols[i];
                if (i == cols.Count - 1) colspan = 0; // the last column will not have colspan
                if (asset.GetType().GetProperty(column.PropertyName) != null)
                {
                    var propertyValue = asset.GetType().GetProperty(column.PropertyName).GetValue(asset);
                    string propertyString = "";

                    if (propertyValue is DateTime) propertyString = Convert.ToDateTime(propertyValue).ToString("yyyy-MM-dd");
                    else if (propertyValue is double) propertyString = TextAlign(FormatN(Convert.ToDouble(propertyValue)), column.Width, "right");
                    else propertyString = propertyValue.ToString();

                    // If the content is longer than the column width, cut it down and add "~"
                    if (propertyString.Length > column.Width) propertyString = propertyString.Substring(0, column.Width - 1) + "~";

                    row += propertyString + new string(' ', Math.Max(column.Width - propertyString.Length + colspan, 0));
                }
                else row += new string(' ', column.Width + colspan);
            }
            return row;
        }


        // --------------- HELPER METHODS ---------------

        // Populate the Context.Assets table with some dummy data and update the local 'assets' List
        static void AddTestData()
        {
            Context.Assets.Add(new Asset("Phone", "Apple", "iPhone XS", "Sweden", DateTime.Now, 999));
            Context.Assets.Add(new Asset("Phone", "Apple", "iPhone X", "Spain", DateTime.Now.AddYears(-3).AddMonths(2), 888));
            Context.Assets.Add(new Asset("Computer", "Apple", "MacBook Pro", "United Kingdom", DateTime.Now.AddYears(-1), 15000));
            Context.Assets.Add(new Asset("Computer", "Lenovo", "ThinkCentre", "Spain", DateTime.Now.AddYears(-3).AddMonths(5), 5000));
            Context.Assets.Add(new Asset("Phone", "Samsung", "Galaxy X", "United Kingdom", DateTime.Now.AddYears(-1), 888));
            Context.Assets.Add(new Asset("Phone", "Samsung", "Galaxy XS", "United Kingdom", DateTime.Now.AddYears(-7), 888));
            Context.Assets.Add(new Asset("Computer", "Asus", "W234", "USA", DateTime.Now.AddYears(-1), 888));
            Context.SaveChanges();
            assets = Context.Assets.ToList();
        }

        // Generates a panel window and draws it onto the consol to show output in a nice way
        static void Panel(string partToPrint, string content = "",
            int width = 0, int hMargin = 0, int tMargin = 0, int bMargin = 0, int hPadding = 2, int vPadding = 0, int border = 1, int colspan = 1,
            string textAlign = "", string color = "", string fontColor = "",
            bool highlight = false, string highlightColor = "", string highlightTextColor = "",
            string subheading = "", List<Column> cols = null, List<Asset> rows = null, bool filterTable = false)
        {
            if (width == 0) width = Console.WindowWidth - (hMargin * 2) - (hPadding * 2) - 2; // sets the panel to full window width if no width is defined
            if (hMargin == 0) hMargin = ((Console.WindowWidth - hMargin - width - hPadding) / 2) - (border * 2); // centers the panel if no hMargin is defined

            content = TextAlign(content, width, textAlign);
            ConsoleColor panelColor, textColor, panelColorHighlight, textColorHighlight;
            if (Enum.TryParse(color, out panelColor)) { }
            else panelColor = ConsoleColor.Gray;
            if (Enum.TryParse(fontColor, out textColor)) { }
            else textColor = ConsoleColor.Black;
            if (highlightColor != "" || highlightTextColor != "") highlight = true;
            if (highlight)
            {
                if (Enum.TryParse(highlightColor, out panelColorHighlight)) { }
                else panelColorHighlight = ConsoleColor.DarkGreen;
                if (Enum.TryParse(highlightTextColor, out textColorHighlight)) { }
                else textColorHighlight = ConsoleColor.White;
            }
            else
            {
                panelColorHighlight = panelColor;
                textColorHighlight = textColor;
            }
            if (tMargin != 0) for (int i = 0; i < tMargin; i++) Console.WriteLine("");
            switch (partToPrint.ToLower())
            {
                case "top":
                    Console.ForegroundColor = panelColor;
                    Console.Write(new string(' ', hMargin));
                    if (border == 1) Console.WriteLine("┌" + new string('─', width + (hPadding * 2)) + "┐");
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "bottom":
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    Console.Write(new string(' ', hMargin));
                    Console.ForegroundColor = panelColor;
                    if (border == 1) Console.WriteLine("└" + new string('─', width + (hPadding * 2)) + "┘");
                    Console.ResetColor();
                    break;
                case "left":
                    Console.ResetColor();
                    Console.Write(new string(' ', hMargin));
                    if (border == 1)
                    {
                        Console.ForegroundColor = panelColor;
                        Console.Write("│");
                        Console.ResetColor();
                    }
                    if (highlight)
                    {
                        Console.BackgroundColor = panelColorHighlight;
                        Console.ForegroundColor = textColorHighlight;
                    }
                    else
                    {
                        Console.BackgroundColor = panelColor;
                        Console.ForegroundColor = textColor;
                    }
                    Console.Write(new string(' ', hPadding));
                    break;
                case "right":
                    Console.Write(new string(' ', Math.Max(width + hMargin + hPadding + 1 - Console.CursorLeft, 0)));
                    Console.Write(new string(' ', hPadding));
                    Console.ResetColor();
                    if (border == 1)
                    {
                        Console.ForegroundColor = panelColor;
                        Console.WriteLine("│");
                    }
                    else Console.WriteLine("");
                    break;
                case "row":
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, color: color, fontColor: fontColor);
                    Panel("left", width: width, border: border, color: color, fontColor: fontColor, highlight: highlight, highlightColor: highlightColor, highlightTextColor: highlightTextColor, hMargin: hMargin);
                    Console.Write(content);
                    Panel("right", width: width, border: border, color: color, fontColor: fontColor, hMargin: hMargin);
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, color: color, fontColor: fontColor);
                    break;
                case "hr":
                    Panel("left", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    Console.Write(new string('─', width));
                    Panel("right", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "br":
                    Panel("left", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    Console.Write(" ");
                    Panel("right", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "heading":
                    Panel("top", width: width, vPadding: 1, hMargin: hMargin, color: color, fontColor: fontColor);
                    Panel("row", content, textAlign: "center", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    if (subheading != "")
                    {
                        Panel("br", color: color, width: width, hMargin: hMargin);
                        Panel("row", subheading, textAlign: "center", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    }
                    Panel("bottom", width: width, vPadding: 1, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "table":
                    string topRow = "";
                    int listWidth = 0;
                    for (int i = 0; i < cols.Count; i++)
                    {
                        Column col = cols[i];
                        int topRowColspan = colspan;
                        if (i == cols.Count - 1) topRowColspan = 0;
                        topRow += col.Name + new string(' ', col.Width - col.Name.Length + topRowColspan);
                        listWidth += col.Width + topRowColspan;
                    }
                    Panel("top", width: listWidth, vPadding: 1, color: color, fontColor: fontColor);
                    Panel("row", topRow, width: listWidth, color: color, fontColor: fontColor);
                    Panel("hr", width: listWidth, color: color, fontColor: fontColor);
                    foreach (var item in rows)
                    {
                        if (filterTable)
                        {
                            if (item.EndOfLife(3))
                            {
                                highlightColor = "Red";
                                highlightTextColor = "";

                            }
                            else if (item.EndOfLife(6))
                            {
                                highlightColor = "Yellow";
                                highlightTextColor = "Black";
                            }
                            else
                            {
                                highlightColor = "";
                                highlightTextColor = "";
                            }
                        }
                        Panel("row", PrintAssetByColumns(item, cols, colspan: colspan), width: listWidth, color: color, fontColor: fontColor, filterTable: filterTable, highlightColor: highlightColor, highlightTextColor: highlightTextColor);
                    }
                    Panel("bottom", width: listWidth, vPadding: 1, color: color, fontColor: fontColor);
                    break;
            }
            if (bMargin != 0) for (int i = 0; i < bMargin; i++) Console.WriteLine("");
        }

        // Returns a string that aligns the passed text inside the width of a containing box
        static string TextAlign(string text = "", int boxLength = 1, string textAlign = "")
        {
            int leftPadding = 0;
            switch (textAlign.ToLower().Trim())
            {
                case "right" or "r":
                    leftPadding = boxLength - text.Length;
                    break;
                case "center" or "c":
                    leftPadding = (boxLength - text.Length) / 2;
                    break;
            }
            return new string(' ', Math.Max(leftPadding, 0)) + text;
        }

        // adds a thousands separator to numbers
        static string FormatN(double number, int decimals = 0)
        {
            return number.ToString("N" + decimals, CultureInfo.InvariantCulture);
        }

        // Displays a menu UI for the options listed in the specified "menu" List
        static void ShowMenu(List<MenuFunction> menu, int selected = 1, string subheading = "", int width = 38)
        {
            Panel("top", width: width, vPadding: 1);
            if (selected < 1) selected = menu.Count;
            else if (selected > menu.Count) selected = 1;
            for (int i = 0; i < menu.Count; i++)
            {
                Panel("row", "[" + (i + 1) + "] " + menu[i].Description, highlight: i == selected - 1, width: width);
            }
            Panel("bottom", width: width, vPadding: 1);
            Console.CursorVisible = false;
            SelectMenu(menu, selected, subheading);
        }

        // Implements redrawing the menu onto the console to give the illusion of up-down selection
        static void SelectMenu(List<MenuFunction> menu, int selected = 1, string subheading = "")
        {
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);
            switch (keyPressed.Key)
            {
                case ConsoleKey.Enter:
                    menu[selected - 1].Action.Invoke();
                    break;
                case ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.Backspace:
                    ShowState(menu, selected - 1, subheading, filterTableTemp, orderByTemp, thenByTemp);
                    break;
                case ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.Tab:
                    ShowState(menu, selected + 1, subheading, filterTableTemp, orderByTemp, thenByTemp);
                    break;
                default: // If the keyPressed is not arrows/Enter, then check which number it is
                    Int32 keyNumber;
                    if (Int32.TryParse(keyPressed.KeyChar.ToString(), out keyNumber) && keyNumber <= menu.Count)
                    {
                        menu[keyNumber - 1].Action.Invoke();
                    }
                    else ShowState(menu, selected, subheading, filterTableTemp, orderByTemp, thenByTemp);
                    break;
            }
        }

        // Checks if the user inputs a term indicating they want to quit
        public static bool wantsToQuit(string userInput)
        {
            switch (userInput.ToLower().Trim())
            {
                case "exit" or "quit" or "q": return true;
                default: return false;
            }
        }
    }

}