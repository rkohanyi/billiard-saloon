using System;
using System.Runtime.Serialization;

namespace BilliardSaloon
{
    class BilliardSaloonException : Exception
    { }

    class NoFreeTableException : BilliardSaloonException
    { }

    class NoSuchReservationNumberException: BilliardSaloonException
    { }

    class Product
    {
        public string Name { get; }
        public int Price { get; }
        public ProductType Type { get; }

        public Product(string name, int price, ProductType type)
        {
            Name = name;
            Price = price;
            Type = type;
        }
    }

    enum ProductType
    {
        DRINK,
        FOOD,
    }

    enum TableType
    {
        SNOOKER,
        REX,
        BILLIARD,
    }

    class Table
    {
        public int Id { get; }
        public TableType Type { get; }

        public bool Occupied { get; private set; }

        private IList<Product> orders = new List<Product>();

        public Table(int id, TableType type)
        {
            Id = id;
            Type = type;
        }

        public void AddOrder(Product product)
        {
            orders.Add(product);
        }

        public int CalculateCost()
        {
            int cost = 0;
            switch(Type)
            {
                case TableType.BILLIARD:
                    {
                        cost += 1200;
                        break;
                    }
                case TableType.SNOOKER:
                    {
                        cost += 800;
                        break;
                    }
                case TableType.REX:
                    {
                        cost += 600;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            foreach (Product product in orders)
            {
                cost += product.Price;
            }
            return cost;
        }

        public void Reserve()
        {
            Occupied = true;
        }

        public int FreeUp()
        {
            Occupied = false;
            int cost = CalculateCost();
            orders.Clear();
            return cost;
        }

        public override string ToString()
        {
            return Id + " (" + Type + ")";
        }
    }

    class Saloon
    {
        private IList<Table> tables = new List<Table>();

        public Saloon(IList<Table> tables)
        {
            this.tables = tables;
        }

        public int ReserveTable(TableType type)
        {
            var freeTables = GetFreeTablesByType(type);
            if (freeTables.Count == 0)
            {
                throw new NoFreeTableException();
            }
            var freeTable = freeTables[0];
            freeTable.Reserve();
            return freeTable.Id;
        }

        public void Order(int reservationNumber, Product product)
        {
            var table = GetTableByReservationNumber(reservationNumber);
            table.AddOrder(product);
        }

        public int Leave(int reservationNumber)
        {
            var table = GetTableByReservationNumber(reservationNumber);
            return table.FreeUp();
        }

        public IList<Table> GetFreeTablesByType(TableType type)
        {
            IList<Table> freeTables = new List<Table>();
            foreach (Table table in tables)
            {
                if (table.Type == type && !table.Occupied)
                {
                    freeTables.Add(table);
                }
            }
            return freeTables;
        }

        public Table GetReservedTablesWithHighestConsumption()
        {
            int highestConsumption = 0;
            Table tableWithHighestConsumption = null;
            foreach (Table table in tables)
            {
                int cost = table.CalculateCost();
                if (table.Occupied && cost > highestConsumption || tableWithHighestConsumption == null)
                {
                    tableWithHighestConsumption = table;
                }
            }
            return tableWithHighestConsumption;
        }

        private Table GetTableByReservationNumber(int reservationNumber)
        {
            foreach (Table table in tables)
            {
                if (table.Id == reservationNumber)
                {
                    return table;
                }
            }
            throw new NoSuchReservationNumberException();
        }
    }

    class Program
    {
        static void Main()
        {
            IList<Table> tables = new List<Table>();

            tables.Add(new Table(1, TableType.SNOOKER));
            tables.Add(new Table(2, TableType.SNOOKER));
            tables.Add(new Table(3, TableType.BILLIARD));
            tables.Add(new Table(4, TableType.BILLIARD));
            tables.Add(new Table(5, TableType.BILLIARD));
            tables.Add(new Table(6, TableType.REX));

            Saloon saloon = new Saloon(tables);
            while (true)
            {
                try
                {

                    Console.WriteLine(@"
1 - Reserver table
2 - Order food/drink
3 - Find free table(s)
4 - Find highest consumption table(s)
0 - Exit
Choose option: 
                ");

                    string input = Console.ReadLine();
                    if (input == "1")
                    {
                        Console.WriteLine("Enter game type: ");
                        var type = (TableType)Enum.Parse(typeof(TableType), Console.ReadLine());
                        int reservationNumber = saloon.ReserveTable(type);
                        Console.WriteLine("Your reservation number is: " + reservationNumber);
                    }
                    else if (input == "2")
                    {
                        Console.WriteLine("Enter reservation number: ");
                        int reservationNumber = int.Parse(Console.ReadLine());
                        Console.WriteLine("Enter your order: ");
                        string order = Console.ReadLine();
                        Console.WriteLine("Enter price: ");
                        int price = int.Parse(Console.ReadLine());
                        Console.WriteLine("Enter type: ");
                        var type = (ProductType)Enum.Parse(typeof(ProductType), Console.ReadLine());
                        Product product = new Product(order, price, type);
                        saloon.Order(reservationNumber, product);
                    }
                    else if (input == "3")
                    {
                        Console.WriteLine("Enter game type: ");
                        var type = (TableType)Enum.Parse(typeof(TableType), Console.ReadLine());
                        var freeTables = saloon.GetFreeTablesByType(type);
                        foreach (Table table in freeTables)
                        {
                            Console.WriteLine(table);
                        }
                    }
                    else if (input == "4")
                    {
                        Table table = saloon.GetReservedTablesWithHighestConsumption();
                        Console.WriteLine(table);
                    }
                    else if (input == "0")
                    {
                        return;
                    }

                }
                catch (BilliardSaloonException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
