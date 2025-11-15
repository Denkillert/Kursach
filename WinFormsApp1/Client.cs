using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RowingBaseAccounting
{

    // Класс клиента
    public class Client : IFileEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Password { get; set; }
        public decimal Balance { get; set; } // НОВОЕ ПОЛЕ: баланс клиента

        public string CompanyName { get; set; }
        public string TaxId { get; set; }

        public Client()
        {
            RegistrationDate = DateTime.Now;
            Balance = 0; // Начальный баланс
        }

        public string ToFileString()
        {
            if (Type == "Юр.лицо")
                return $"{Id}|{Name}|{Type}|{Phone}|{Email}|{DiscountPercent}|{CompanyName}|{TaxId}|{RegistrationDate:dd.MM.yyyy HH:mm}|{DateOfBirth:dd.MM.yyyy}|{Password}|{Balance}";
            else
                return $"{Id}|{Name}|{Type}|{Phone}|{Email}|{DiscountPercent}|{RegistrationDate:dd.MM.yyyy HH:mm}|{DateOfBirth:dd.MM.yyyy}|{Password}|{Balance}";
        }

        public void FromFileString(string data)
        {
            var parts = data.Split('|');
            Id = int.Parse(parts[0]);
            Name = parts[1];
            Type = parts[2];
            Phone = parts[3];
            Email = parts[4];
            DiscountPercent = decimal.Parse(parts[5]);

            int index = 6;
            if (Type == "Юр.лицо" && parts.Length > index)
            {
                CompanyName = parts[index++];
                TaxId = parts[index++];
            }

            if (parts.Length > index && DateTime.TryParse(parts[index], out DateTime regDate))
            {
                RegistrationDate = regDate;
                index++;
            }

            if (parts.Length > index && DateTime.TryParse(parts[index], out DateTime dob))
            {
                DateOfBirth = dob;
                index++;
            }

            // Чтение пароля
            if (parts.Length > index)
            {
                Password = parts[index];
                index++;
            }

            // Чтение баланса (новое поле)
            if (parts.Length > index && decimal.TryParse(parts[index], out decimal balance))
            {
                Balance = balance;
            }
        }

        public bool CanUseService(Service service)
        {
            if (service.IsForLegalEntitiesOnly && Type != "Юр.лицо")
                return false;
            return true;
        }

        // Новый метод для пополнения баланса
        public void AddBalance(decimal amount)
        {
            if (amount > 0)
            {
                Balance += amount;
            }
        }

        // Новый метод для списания средств
        public bool DeductBalance(decimal amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }
            return false;
        }
    }

    // Менеджер клиентов
    public static class ClientManager
    {
        private static List<Client> clients = new List<Client>();

        public static void AddClient(Client client)
        {
            client.Id = clients.Count > 0 ? clients.Max(c => c.Id) + 1 : 1;

            client.DiscountPercent = client.Type switch
            {
                "Студент" => 30,
                "Сотрудник" => 20,
                _ => 0
            };

            // Если пароль не установлен, устанавливаем телефон как пароль по умолчанию
            if (string.IsNullOrEmpty(client.Password))
            {
                client.Password = client.Phone;
            }

            clients.Add(client);
        }

        public static List<Client> GetClients() => clients;

        public static Client GetClient(int id) => clients.FirstOrDefault(c => c.Id == id);

        public static Client FindClient(string name, DateTime dateOfBirth)
        {
            return clients.FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.DateOfBirth.HasValue &&
                c.DateOfBirth.Value.Date == dateOfBirth.Date);
        }

        public static Client FindClientByPhone(string phone)
        {
            return clients.FirstOrDefault(c => c.Phone == phone);
        }

        public static void SetClientDiscount(int clientId, decimal discountPercent)
        {
            var client = GetClient(clientId);
            if (client != null)
                client.DiscountPercent = discountPercent;
        }

        public static void SetClientTypeDiscount(string clientType, decimal discountPercent)
        {
            foreach (var client in clients.Where(c => c.Type == clientType))
            {
                client.DiscountPercent = discountPercent;
            }
        }

        // НОВЫЙ МЕТОД: Пополнение баланса клиента
        public static bool AddBalanceToClient(int clientId, decimal amount)
        {
            var client = GetClient(clientId);
            if (client != null && amount > 0)
            {
                client.AddBalance(amount);
                return true;
            }
            return false;
        }

        // НОВЫЙ МЕТОД: Списание средств с баланса клиента
        public static bool DeductBalanceFromClient(int clientId, decimal amount)
        {
            var client = GetClient(clientId);
            if (client != null)
            {
                return client.DeductBalance(amount);
            }
            return false;
        }

        // НОВЫЙ МЕТОД: Получение баланса клиента
        public static decimal GetClientBalance(int clientId)
        {
            var client = GetClient(clientId);
            return client?.Balance ?? 0;
        }
    }
}
