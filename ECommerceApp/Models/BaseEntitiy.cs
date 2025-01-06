namespace ECommerceApp.Models
{
    public class BaseEntitiy
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime LastUpdateDate {  get; set; } = DateTime.Now;

        public string? LastUpdateUserId { get; set; }

        public void SetCreatedDate()
        {
            CreatedDate = DateTime.Now;
        }

        public void SetUpdatedDate(string userId) // userId'yi string olarak kabul edecek
        {
            LastUpdateDate = DateTime.Now;
            LastUpdateUserId = userId; // LastUpdateUserId de string olmalı
        }

    }


}
