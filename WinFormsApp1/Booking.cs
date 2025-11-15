using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RowingBaseAccounting
{

    // Класс бронирования
    // Класс бронирования (остается без изменений)
    public class Booking : IFileEntity
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartTime { get; set; }
        public double Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = "Активно";
        public int BaseNumber { get; set; }
        public string BookingType { get; set; } = "Разовое";
        public int? RemainingSessions { get; set; }
        public int? OriginalSessions { get; set; }
        public bool IsPaid { get; set; } // НОВОЕ ПОЛЕ: подтверждение оплаты

        public DateTime EndTime
        {
            get
            {
                if (ServiceName.Contains("терраса") || ServiceName.Contains("сауна"))
                    return StartTime.AddHours(2 * Quantity);
                else
                    return StartTime.AddHours(Quantity);
            }
        }

        public string ToFileString()
        {
            return $"{Id}|{ClientId}|{ServiceName}|{StartTime:dd.MM.yyyy HH:mm}|{Quantity}|{TotalCost}|{Status}|{BaseNumber}|{BookingType}|{RemainingSessions}|{OriginalSessions}|{IsPaid}";
        }

        public void FromFileString(string data)
        {
            var parts = data.Split('|');
            Id = int.Parse(parts[0]);
            ClientId = int.Parse(parts[1]);
            ServiceName = parts[2];
            StartTime = DateTime.ParseExact(parts[3], "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            Quantity = double.Parse(parts[4]);
            TotalCost = decimal.Parse(parts[5]);
            Status = parts[6];
            BaseNumber = int.Parse(parts[7]);

            BookingType = parts.Length > 8 ? parts[8] : "Разовое";
            RemainingSessions = parts.Length > 9 && int.TryParse(parts[9], out int rem) ? rem : (int?)null;
            OriginalSessions = parts.Length > 10 && int.TryParse(parts[10], out int orig) ? orig : (int?)null;
            IsPaid = parts.Length > 11 && bool.TryParse(parts[11], out bool paid) ? paid : false;
        }

        public bool IsMembership()
        {
            return BookingType == "Абонемент" && RemainingSessions.HasValue;
        }

        public bool UseSession()
        {
            if (!IsMembership() || RemainingSessions <= 0)
                return false;

            RemainingSessions--;
            if (RemainingSessions == 0)
                Status = "Использован";

            return true;
        }
    }

    // Менеджер бронирований с исправленными методами
    public static class BookingManager
    {
        private static List<Booking> bookings = new List<Booking>();

        public static bool CreateBooking(Client client, Service service, DateTime startTime, double quantity)
        {
            // Проверка доступности для юр.лиц
            if (service.IsForLegalEntitiesOnly && client.Type != "Юр.лицо")
            {
                MessageBox.Show("Эта услуга доступна только для юридических лиц");
                return false;
            }

            // Проверка максимального количества посетителей (30 человек)
            if (GetCurrentVisitorsCount(startTime, service.BaseNumber) >= 30)
            {
                MessageBox.Show("Превышено максимальное количество посетителей (30 человек) в это время");
                return false;
            }

            if (!service.Name.Contains("абонемент") && IsServiceBooked(service.Name, service.BaseNumber, startTime, quantity))
                return false;

            var booking = new Booking
            {
                Id = bookings.Count > 0 ? bookings.Max(b => b.Id) + 1 : 1,
                ClientId = client.Id,
                ServiceName = service.Name,
                StartTime = startTime,
                Quantity = quantity,
                TotalCost = service.CalculateCost(client, quantity),
                BaseNumber = service.BaseNumber,
                IsPaid = false // По умолчанию не оплачено
            };

            if (service.ServiceType == "Абонемент")
            {
                booking.BookingType = "Абонемент";
                booking.RemainingSessions = service.SessionsCount;
                booking.OriginalSessions = service.SessionsCount;
                booking.Status = "Активно";
            }

            bookings.Add(booking);
            return true;
        }

        // Новый метод для подтверждения оплаты
        public static bool ConfirmPayment(int bookingId)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.IsPaid = true;
                return true;
            }
            return false;
        }

        // Новый метод для отмены оплаты
        public static bool CancelPayment(int bookingId)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.IsPaid = false;
                return true;
            }
            return false;
        }

        // Новый метод для получения неоплаченных бронирований
        public static List<Booking> GetUnpaidBookings(int baseNumber)
        {
            return bookings.Where(b => b.BaseNumber == baseNumber && !b.IsPaid && b.Status == "Активно").ToList();
        }

        // Новый метод для подсчета текущих посетителей
        private static int GetCurrentVisitorsCount(DateTime startTime, int baseNumber)
        {
            DateTime endTime = startTime.AddHours(2);
            return GetBookings(baseNumber)
                .Count(b => b.Status == "Активно" &&
                           startTime < b.EndTime &&
                           endTime > b.StartTime);
        }

        public static bool UseMembership(int clientId, string serviceName, DateTime startTime)
        {
            string normalizedService = serviceName.ToLower().Replace('ё', 'е');

            var membership = bookings.FirstOrDefault(b =>
                b.ClientId == clientId &&
                b.IsMembership() &&
                b.Status == "Активно" &&
                b.RemainingSessions > 0 &&
                b.ServiceName.ToLower().Replace('ё', 'е').Contains("тренажер"));

            if (membership == null)
                return false;

            if (!membership.UseSession())
                return false;

            var usage = new Booking
            {
                Id = bookings.Count > 0 ? bookings.Max(b => b.Id) + 1 : 1,
                ClientId = clientId,
                ServiceName = serviceName,
                StartTime = startTime,
                Quantity = 1,
                TotalCost = 0,
                Status = "Активно",
                BaseNumber = membership.BaseNumber,
                BookingType = "По абонементу",
                IsPaid = true // По абонементу автоматически считается оплаченным
            };

            bookings.Add(usage);
            return true;
        }

        public static bool HasActiveMembership(int clientId)
        {
            return bookings.Any(b =>
                b.ClientId == clientId &&
                b.IsMembership() &&
                b.Status == "Активно" &&
                b.RemainingSessions > 0);
        }

        public static void DisplayClientMemberships(int clientId)
        {
            var memberships = bookings.Where(b =>
                b.ClientId == clientId &&
                b.IsMembership()).ToList();

            if (memberships.Count == 0)
            {
                Console.WriteLine("У клиента нет абонементов.");
                return;
            }
        }

        public static List<Booking> GetClientBookings(int clientId)
        {
            return bookings.Where(b => b.ClientId == clientId && b.Status == "Активно").ToList();
        }

        public static bool CancelBooking(int bookingId, int clientId)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId && b.ClientId == clientId);
            if (booking != null && (booking.StartTime - DateTime.Now).TotalHours > 48)
            {
                booking.Status = "Отменено клиентом";
                return true;
            }
            return false;
        }

        // НОВЫЙ МЕТОД: Полное удаление бронирования
        public static bool DeleteBooking(int bookingId)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                bookings.Remove(booking);
                return true;
            }
            return false;
        }

        private static bool IsServiceBooked(string serviceName, int baseNumber, DateTime startTime, double quantity)
        {
            DateTime endTime = startTime.AddHours(quantity);
            return bookings.Any(b =>
                b.ServiceName == serviceName &&
                b.BaseNumber == baseNumber &&
                b.Status == "Активно" &&
                startTime < b.EndTime &&
                endTime > b.StartTime);
        }

        public static List<Booking> GetBookings() => bookings;
        public static List<Booking> GetBookings(int baseNumber) => bookings.Where(b => b.BaseNumber == baseNumber).ToList();

        public static decimal CalculateRevenue(DateTime startDate, DateTime endDate, int baseNumber)
        {
            return GetBookings(baseNumber)
                .Where(b => b.StartTime >= startDate && b.EndTime <= endDate && b.Status == "Активно" && b.IsPaid)
                .Sum(b => b.TotalCost);
        }

        // НОВЫЕ МЕТОДЫ ДЛЯ АНАЛИТИКИ
        public static List<Booking> GetBookingsByPeriod(DateTime startDate, DateTime endDate, int baseNumber)
        {
            return GetBookings(baseNumber)
                .Where(b => b.StartTime >= startDate && b.EndTime <= endDate)
                .ToList();
        }

        public static Dictionary<string, int> GetServicePopularity(DateTime startDate, DateTime endDate, int baseNumber)
        {
            var periodBookings = GetBookingsByPeriod(startDate, endDate, baseNumber);
            return periodBookings
                .GroupBy(b => b.ServiceName)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public static Dictionary<int, int> GetPeakHours(DateTime startDate, DateTime endDate, int baseNumber)
        {
            var periodBookings = GetBookingsByPeriod(startDate, endDate, baseNumber);
            var hourCounts = new Dictionary<int, int>();

            for (int hour = 0; hour < 24; hour++)
            {
                hourCounts[hour] = periodBookings.Count(b => b.StartTime.Hour == hour);
            }

            return hourCounts;
        }

        // НОВЫЕ МЕТОДЫ ДЛЯ АДМИНИСТРАТОРА
        public static bool AdminCancelBooking(int bookingId)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.Status = "Отменено администратором";
                return true;
            }
            return false;
        }

        public static void UpdateBooking(int bookingId, DateTime newStartTime, double newQuantity)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.StartTime = newStartTime;
                booking.Quantity = newQuantity;
            }
        }
    }
}
