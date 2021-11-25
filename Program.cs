using System;
using System.IO;
using System.Threading;

namespace Super_Market
{
    class Program
    {
        //user login class  and constr.
        public class Users
        {
            public string username;
            public string password;
            private int id;
            public Users(string username, string password, int id)
            {
                this.username = username;
                this.password = password;
                this.id = id;
            }
        }

        static void Main(string[] args)
        {
            //A method that display our group members name and Id
            splashScreen();

            //login Screen and 
            Login();
        }


        static public void Sort(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileStream readerStream = new FileStream(fileName, FileMode.Open);
                string[] content = null;

                //Read the content
                using (StreamReader reader = new StreamReader(readerStream))
                {
                    content = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    //Remove the entries in the file
                    readerStream.SetLength(0);
                }

                FileStream writerStream = new FileStream(fileName, FileMode.OpenOrCreate);
                using (StreamWriter writer = new StreamWriter(writerStream))
                {
                    //Sort the content and write back to the same file
                    Array.Sort(content);
                    writer.Write(string.Join(Environment.NewLine, content));
                }
                Console.WriteLine("\t---------------------------------------");
                Console.WriteLine("\t    Product seccussfuly Sorted!");
                Console.WriteLine("\t---------------------------------------");
            }
            else
            {
                Console.WriteLine("\tFile not exist!");
            }
        }

        //homepage of users(Costomer) and Seller
        static public void HomePage()
        {
            String exit;
            var Choice = "";
            do
            {
            TryAgain:
                Console.WriteLine("\tWELCOME TO OUR SUPER MARKET!");
                Console.WriteLine("\tMAIN MENU");
                Console.WriteLine("\t1. Buy Product");
                Console.WriteLine("\t2. Logout");
                Console.WriteLine("\t0. Exit");
                Console.Write("\n\tEnter your Choice: ");
                Choice = Console.ReadLine();

                if (Choice == "1")
                {
                    Console.Clear();
                openAgain:
                    //buy product method
                    BuyProduct();

                    Console.Write("\n\tDo You Want To Continue? [y/n] ");
                    exit = Console.ReadLine();
                    if (exit == "y" || exit == "Y")
                    {
                        goto openAgain;
                    }
                    else
                    {
                        PayBill("OrderList.txt", 1);
                    }
                }
                else if (Choice == "2")
                {
                    Console.Clear();
                    Login();
                }
                else if (Choice == "0")
                {
                    break;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("\tInvalid choice! Try again <0_2>!");
                    goto TryAgain;
                }
            } while (Choice != "0");
        }

        private static void PayBill(string fileName, int deleteFlag)
        {
            int Singleprice = 0, TotalAmount = 0, TotalPrice = 0;
            double TotalTax = 0.0;
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string oneLine = "";
                    Console.Write("\n\t-----------------------------------------------------\n");
                    Console.Write("\t|{0} : {1} ", "Super Market Billing System ", DateTime.Now.ToString());
                    Console.Write("\n\t-----------------------------------------------------\n");
                    Console.Write("\t| {0}  | {1} |{2}  | {3}   | {4}  |", "proId", "ProName", "Price", "ExpireDate", "Amount");
                    while ((oneLine = sr.ReadLine()) != null)
                    {
                        var col = oneLine.Split(',');
                        Singleprice = int.Parse(col[2]) * int.Parse(col[4]);
                        TotalPrice += Singleprice;
                        TotalAmount += int.Parse(col[4]);

                        Console.Write("\n\t-----------------------------------------------------\n");
                        Console.Write("\t| {0}  | {1}  | {2}    | {3}   | {4}     |", col[0], col[1], col[2], col[3], col[4]);
                    }
                    TotalTax = CalculateTax(TotalPrice);

                    Console.Write("\n\t-----------------------------------------------------\n");
                    Console.Write("\t|{0}{1}| {2}{3}| {4}{5}| {6}{7} |\n\n", "TTax: ", TotalTax, "Price: ", TotalPrice, "TPrice: ", (TotalPrice + TotalTax), "TAmount: ", TotalAmount);
                }
                if (deleteFlag == 1)
                {
                    File.Delete(fileName);
                }


            }
            else
            {
                Console.WriteLine("\t Please add product to pay bill!");
            }
        }

        private static void BuyProduct()
        {
            string itemId;
            int amount = 0;
            string productFile = "Products.txt";
            if (File.Exists(productFile))
            {
                Console.Clear();
            open:
                DisplayAllProduct(productFile);
                Console.Write("\n\n\tAdd to Order list [Use Product Id]: ");
                itemId = Console.ReadLine();
                if (SearchProduct(itemId, "", 0, 0) == 1)
                {
                    Console.Write("\n\tHow much do u want: ");
                    amount = int.Parse(Console.ReadLine());
                    AddToOrderList(itemId, amount);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("\tProduct not available Please Try Again!");
                    goto open;
                }
            }
            else
            {
                Console.Write("\tSuper Market Closed!");
            }
        }

        private static void AddToOrderList(string itemId, int amount)
        {

            string ProFile = "Products.txt";
            string OrderListFile = "OrderList.txt";
            using (StreamReader sr = File.OpenText(ProFile))
            {
                string oneLine = "";
                while ((oneLine = sr.ReadLine()) != null)
                {
                    string[] col = oneLine.Split(',');
                    if (col[0] == itemId)
                    {
                        if (int.Parse(col[4]) >= amount)
                        {
                            String availableAmount = (int.Parse(col[4]) - amount).ToString();
                            ModifyProduct(col[0], 1, availableAmount);//modify amount of product
                            ADDPurchaseHistory(col[0]);//add to purchase history

                            if (File.Exists(OrderListFile))
                            {
                                using (StreamWriter w = File.AppendText(OrderListFile))
                                {
                                    w.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], amount);
                                }
                            }
                            else
                            {
                                using (StreamWriter sw = File.CreateText(OrderListFile))
                                {
                                    sw.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], amount);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("\t{0} amount of {1} is not available!", amount, col[1]);
                        }

                    }
                }
            }
        }

        //Admin panel method
        static public void AdminPanel()
        {
            int Choice, Amount;
            string id, name;
            string productFile = "Products.txt";

        TryAgain:
            do
            {
                Console.WriteLine("\n\n\tWELCOME TO OUR SUPER MARKET ADMIN PANEL!");
                Console.WriteLine("\tADMIN MENU");
                Console.WriteLine("\t1. Add Product");
                Console.WriteLine("\t2. Display All Product");
                Console.WriteLine("\t3. Modify Product");
                Console.WriteLine("\t4. Delete Product");
                Console.WriteLine("\t5. Search Product By Id");
                Console.WriteLine("\t6. Search Product By Name");
                Console.WriteLine("\t7. Sort by Id");
                Console.WriteLine("\t8. Display Purchase History");
                Console.WriteLine("\t9. Logout");
                Console.WriteLine("\t0. Exit");
                Console.Write("\n\tEnter your Choice: ");
                Choice = int.Parse(Console.ReadLine());

                //   DateTime.Now.ToString()
                //   // Write file contents on console.     

                if (Choice == 1)
                {
                    Console.Clear();
                openAgain:
                    Console.Write("\tHow many products do you want to Add? ");
                    Amount = int.Parse(Console.ReadLine());
                    if (Amount != 0)
                    {
                        AddProduct(Amount);
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("\tInvalid Input! please try again!");
                        goto openAgain;
                    }
                }
                else if (Choice == 2)
                {
                    Console.Clear();
                    DisplayAllProduct(productFile);

                }
                else if (Choice == 3)
                {
                open:
                    Console.Clear();
                    Console.Write("\tEnter product id to Modify: ");
                    id = Console.ReadLine();
                    if (SearchProduct(id, "", 0, 0) == 1)
                    {
                        ModifyProduct(id, 0, "");

                    }
                    else
                    {
                        Console.Write("\tProduct not available! please try again!");
                        Console.ReadKey();
                        goto open;
                    }
                }
                else if (Choice == 4)
                {

                open:
                    Console.Clear();
                    Console.Write("\tEnter product id to delete: ");
                    id = Console.ReadLine();
                    if (SearchProduct(id, "", 0, 0) == 1)
                    {
                        DeleteProduct(id);

                    }
                    else
                    {
                        Console.Write("\tProduct not available! please try again!");
                        Console.ReadKey();
                        goto open;
                    }
                }
                else if (Choice == 5)
                {
                open:
                    Console.Clear();
                    Console.Write("\tEnter product id to search: ");
                    id = Console.ReadLine();
                    if (SearchProduct(id, "", 0, 0) == 1)
                    {
                        SearchProduct(id, "", 1, 0);

                    }
                    else
                    {
                        Console.Write("\tProduct not available! please try again!");
                        Console.ReadKey();
                        goto open;

                    }

                }
                else if (Choice == 6)
                {
                open:
                    Console.Clear();
                    Console.Write("\tEnter product name to search: ");
                    name = Console.ReadLine();
                    if (SearchProduct("", name, 0, 0) == 1)
                    {
                        SearchProduct("", name, 0, 1);

                    }
                    else
                    {
                        Console.Write("\tProduct not available! please try again!");
                        Console.ReadKey();
                        goto open;

                    }

                }
                else if (Choice == 7)
                {
                    Sort("Products.txt");
                }
                else if (Choice == 8)
                {
                    Console.Clear();
                    PayBill("PurchaseHistory.txt", 0);
                }
                else if (Choice == 9)
                {
                    Login();
                }
                else if (Choice == 0)
                {

                }
                else
                {
                    Console.WriteLine("\tInvalid choice! Try again <0_8>!");
                    goto TryAgain;
                }
            } while (Choice != 0);


        }

        static public void ADDPurchaseHistory(string id)
        {
            string fileName = "Products.txt";
            string PurchaseHistory = "PurchaseHistory.txt";
            using (StreamReader sr = File.OpenText(fileName))
            {
                string oneLine = "";
                while ((oneLine = sr.ReadLine()) != null)
                {
                    var col = oneLine.Split(',');
                    if (col[0] == id)
                    {
                        if (File.Exists(PurchaseHistory))
                        {
                            using (StreamWriter w = File.AppendText(PurchaseHistory))
                            {
                                w.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], col[4]);
                            }
                        }
                        else
                        {
                            using (StreamWriter sw = File.CreateText(PurchaseHistory))
                            {
                                sw.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], col[4]);
                            }
                        }
                    }
                }
            }
        }
        static public void AddProduct(int amount)
        {
            string fileName = "Products.txt";
            int n = 1;
            while (n <= amount)
            {
                Console.Clear();
                Console.WriteLine("\t---------------------------------------");
                Console.WriteLine("\tAdd new Product {0}", n);
                Console.WriteLine("\t---------------------------------------");
                Console.Write("\tProduct Id: ");
                int productId = int.Parse(Console.ReadLine());
                Console.WriteLine("\t---------------------------------------");
                Console.Write("\tProduct name: ");
                var productName = Console.ReadLine();
                Console.WriteLine("\t---------------------------------------");
                Console.Write("\tProduct price : ");
                int productPrice = int.Parse(Console.ReadLine());
                Console.WriteLine("\t---------------------------------------");
                Console.Write("\tProduct Expire Date: ");
                var productExpire = Console.ReadLine();
                Console.WriteLine("\t---------------------------------------");
                Console.Write("\tProduct Amount: ");
                int productAmount = int.Parse(Console.ReadLine());
                Console.WriteLine("\t---------------------------------------");
                try
                {
                    // Check if file already exists if exist it appendText     
                    if (File.Exists(fileName))
                    {
                        using (StreamWriter w = File.AppendText(fileName))
                        {
                            w.WriteLine("{0},{1},{2},{3},{4}", productId, productName, productPrice, productExpire, productAmount);

                        }
                    }
                    else
                    {
                        // else Create a new file     
                        using (StreamWriter sw = File.CreateText(fileName))
                        {
                            sw.WriteLine("{0},{1},{2},{3},{4}", productId, productName, productPrice, productExpire, productAmount);
                        }
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }
                n++;
            }
            Console.WriteLine("\t---------------------------------------");
            Console.WriteLine("\t{0} Product seccussfuly Added!", n - 1);
            Console.WriteLine("\t---------------------------------------");

        }
        static public void DisplayAllProduct(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string oneLine = "";
                    Console.Write("\n\t-----------------------------------------------------\n");
                    Console.Write("\t| {0}  | {1} |{2}  | {3}   | {4}  |", "proId", "ProName", "Price", "ExpireDate", "Amount");
                    while ((oneLine = sr.ReadLine()) != null)
                    {
                        var col = oneLine.Split(',');
                        Console.Write("\n\t-----------------------------------------------------\n");
                        Console.Write("\t| {0}  | {1}  | {2}  | {3}  | {4}     |", col[0], col[1], col[2], col[3], col[4]);
                    }
                }
            }
            else
            {
                Console.WriteLine("\tSuper market closed!");
            }
        }
        static public void ModifyProduct(String id, int SystemEdit, String AvailableProductAmount)
        {
            string oldFile = "Products.txt";
            string NewFile = "temp.txt";
            using (StreamReader sr = File.OpenText(oldFile))
            {
                string oneLine = "";
                while ((oneLine = sr.ReadLine()) != null)
                {
                    string[] col = oneLine.Split(',');
                    if (col[0] == id)
                    {
                        if (SystemEdit == 1)
                        {
                            col[4] = AvailableProductAmount;
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("\t---------------------------------------");
                            Console.WriteLine("\tEdit Product ", id);
                            Console.WriteLine("\t---------------------------------------");
                            Console.Write("\tProduct name: ");
                            col[1] = Console.ReadLine();
                            Console.WriteLine("\t---------------------------------------");
                            Console.Write("\tProduct price : ");
                            col[2] = Console.ReadLine();
                            Console.WriteLine("\t---------------------------------------");
                            Console.Write("\tProduct Expire Date: ");
                            col[3] = Console.ReadLine();
                            Console.WriteLine("\t---------------------------------------");
                            Console.Write("\tProduct Amount: ");
                            col[4] = Console.ReadLine();
                            Console.WriteLine("\t---------------------------------------");

                        }

                    }
                    if (File.Exists(NewFile))
                    {
                        using (StreamWriter w = File.AppendText(NewFile))
                        {
                            w.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], col[4]);
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.CreateText(NewFile))
                        {
                            sw.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], col[4]);
                        }
                    }
                }
            }
            File.Delete(oldFile);
            File.Move(NewFile, oldFile);
            if (SystemEdit == 0)
            {
                Console.WriteLine("\t---------------------------------------");
                Console.WriteLine("\tRecord seccussfuly Edited!");
                Console.WriteLine("\t---------------------------------------");
            }



        }
        private static void DeleteProduct(string id)
        {
            string oldFile = "Products.txt";
            string NewFile = "temp.txt";
            using (StreamReader sr = File.OpenText(oldFile))
            {
                string oneLine = "";
                while ((oneLine = sr.ReadLine()) != null)
                {
                    string[] col = oneLine.Split(',');
                    if (col[0] != id)
                    {
                        if (File.Exists(NewFile))
                        {
                            using (StreamWriter w = File.AppendText(NewFile))
                            {
                                w.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], col[4]);
                            }
                        }
                        else
                        {
                            using (StreamWriter sw = File.CreateText(NewFile))
                            {
                                sw.WriteLine("{0},{1},{2},{3},{4}", col[0], col[1], col[2], col[3], col[4]);
                            }
                        }
                    }
                }
            }
            try
            {
                File.Delete(oldFile);
                File.Move(NewFile, oldFile);
            }
            catch (System.Exception)
            {
            }
            Console.WriteLine("\t---------------------------------------");
            Console.WriteLine("\tRecord seccussfuly deleted!");
            Console.WriteLine("\t---------------------------------------");
        }

        //A method that search product by ID, Name and also Display it.
        static public int SearchProduct(string Id, string name, int displayFlagId, int displayFlagName)
        {
            string fileName = "Products.txt";
            using (StreamReader sr = File.OpenText(fileName))
            {
                string oneLine = "";
                while ((oneLine = sr.ReadLine()) != null)
                {
                    var col = oneLine.Split(',');
                    if (col[0] == Id)
                    {
                        if (displayFlagId == 1)
                        {
                            Console.Write("\n\t-----------------------------------------------------\n");
                            Console.Write("\t| {0}  | {1} |{2}  | {3}   | {4}  |", "proId", "ProName", "Price", "ExpireDate", "Amount");
                            Console.Write("\n\t-----------------------------------------------------\n");
                            Console.Write("\t| {0}  | {1}  | {2}  | {3}  | {4}     |", col[0], col[1], col[2], col[3], col[4]);
                        }
                        return 1;
                    }

                    if (col[1] == name)
                    {
                        if (displayFlagName == 1)
                        {
                            Console.Write("\n\t-----------------------------------------------------\n");
                            Console.Write("\t| {0}  | {1} |{2}  | {3}   | {4}  |", "proId", "ProName", "Price", "ExpireDate", "Amount");
                            Console.Write("\n\t-----------------------------------------------------\n");
                            Console.Write("\t| {0}  | {1}  | {2}  | {3}  | {4}     |", col[0], col[1], col[2], col[3], col[4]);

                        }
                        return 1;
                    }
                }
            }
            return 0;

        }
        static public void splashScreen()
        {
            Console.Clear();
            Console.WriteLine("\tMicroLink I.T college Department of Computer Science");
            Console.WriteLine("\n\t\t\t\tSection Two");
            Console.WriteLine("\n\tGROUP MEMBERS: ");
            Console.WriteLine("\tNo  FullName            Id_Num");
            Console.WriteLine("\t1.  Ebisa     Kebede    15026/20");
            Console.WriteLine("\t2.  kalkidan  Alemu     14849/20");
            Console.WriteLine("\t3.  Meymuna   Mohammed  14719/20");
            Console.WriteLine("\t4.  Mayko     Solomon   14689/20\n");
            Thread.Sleep(4000);
        }
        static public void Login()
        {
            int count = 1;//to check how many times user fail password 
            var arrUsers = new Users[] {
            new Users("admin","admin",1),//default admin password
            new Users("csharp","1",2),//default user password
            };
        Start:
            Console.Clear();
            Console.WriteLine("\n\t---------------------------------------");
            Console.WriteLine("\n\t  Login [1] |  SignUp [2]  |  Exit [0]  ");
            Console.WriteLine("\n\t---------------------------------------");
            Console.Write("\tEnter Your Choice: ");
            var input = Console.ReadLine();
            bool successfull = false;
            while (!successfull)
            {
                if (input == "1")
                {
                    Console.WriteLine("\t---------------------------------------");
                    Console.Write("\tWrite your username: ");
                    var username = Console.ReadLine();
                    Console.WriteLine("\t---------------------------------------");
                    Console.Write("\tEnter your password: ");
                    var password = Console.ReadLine();
                    Console.WriteLine("\t---------------------------------------");
                    foreach (Users user in arrUsers)
                    {
                        if (username == user.username && password == user.password)
                        {
                            if (username == "admin" && password == "admin")
                            {
                                Console.Clear();
                                AdminPanel();//if password admin admin AdminPanel execute
                            }
                            else
                            {
                                Console.Clear();
                                HomePage();//if password correct Homepage execute
                            }
                            successfull = true;
                            break;
                        }
                    }

                    if (!successfull)
                    {
                        //if the user try password three more times the program exit
                        if (count == 3)
                        {
                            Console.WriteLine("\tYour username or password is incorect, try again later!!!");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("\tYour username or password is incorect, try again !!!");
                            count++;
                        }
                    }
                }
                else if (input == "0")
                {
                    //exit program
                    break;
                }
                else if (input == "2")
                {
                    //signup button form
                    Console.WriteLine("\t---------------------------------------");
                    Console.Write("\tWrite your username: ");
                    var username = Console.ReadLine();
                    Console.WriteLine("\t---------------------------------------");
                    Console.Write("\tEnter your password: ");
                    var password = Console.ReadLine();
                    Console.WriteLine("\t---------------------------------------");
                    Console.Write("\tEnter your id: ");
                    int id = int.Parse(Console.ReadLine());
                    Console.WriteLine("\t---------------------------------------");

                    //save user info
                    Array.Resize(ref arrUsers, arrUsers.Length + 1);
                    arrUsers[arrUsers.Length - 1] = new Users(username, password, id);
                    successfull = true;
                    goto Start;
                }
                else
                {
                    goto Start;
                }
            }
        }

        static public double CalculateTax(int TotalMoney)
        {
            return (.15 * TotalMoney);
        }
    }
}


