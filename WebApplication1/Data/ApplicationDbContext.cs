using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Entities;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
        public DbSet<Keyword> Keywords => Set<Keyword>();
        public DbSet<BookKeyword> BookKeywords => Set<BookKeyword>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Publisher> Publishers => Set<Publisher>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<StockLedger> StockLedgers => Set<StockLedger>();
        public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
        public DbSet<OutOfStockRequest> OutOfStockRequests => Set<OutOfStockRequest>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Book>(e =>
            {
                e.HasKey(x => x.BookId);
                e.Property(x => x.BookNo).HasMaxLength(32);
                e.Property(x => x.Title).HasMaxLength(200);
                e.Property(x => x.Price).HasPrecision(10, 2);
                e.HasIndex(x => x.BookNo).IsUnique();
            });

            builder.Entity<Author>(e =>
            {
                e.HasKey(x => x.AuthorId);
                e.Property(x => x.AuthorName).HasMaxLength(100);
                e.HasIndex(x => x.AuthorName).IsUnique();
            });

            builder.Entity<BookAuthor>(e =>
            {
                e.HasKey(x => new { x.BookId, x.AuthorOrder });
                e.Property(x => x.AuthorOrder);
                e.HasOne(x => x.Book).WithMany(x => x.Authors).HasForeignKey(x => x.BookId);
                e.HasOne(x => x.Author).WithMany(x => x.Books).HasForeignKey(x => x.AuthorId);
                e.HasIndex(x => new { x.BookId, x.AuthorId }).IsUnique();
            });

            builder.Entity<Keyword>(e =>
            {
                e.HasKey(x => x.KeywordId);
                e.Property(x => x.KeywordText).HasMaxLength(50);
                e.HasIndex(x => x.KeywordText).IsUnique();
            });

            builder.Entity<BookKeyword>(e =>
            {
                e.HasKey(x => new { x.BookId, x.KeywordId });
                e.HasOne(x => x.Book).WithMany(x => x.Keywords).HasForeignKey(x => x.BookId);
                e.HasOne(x => x.Keyword).WithMany(x => x.Books).HasForeignKey(x => x.KeywordId);
            });

            builder.Entity<Supplier>(e =>
            {
                e.HasKey(x => x.SupplierId);
                e.Property(x => x.SupplierName).HasMaxLength(120);
                e.HasIndex(x => x.SupplierName).IsUnique();
            });

            builder.Entity<Publisher>(e =>
            {
                e.HasKey(x => x.PublisherId);
                e.Property(x => x.PublisherName).HasMaxLength(120);
                e.HasIndex(x => x.PublisherName).IsUnique();
            });

            builder.Entity<Order>(e =>
            {
                e.HasKey(x => x.OrderId);
                e.Property(x => x.TotalAmount).HasPrecision(12, 2);
                e.Property(x => x.ReceiverName).HasMaxLength(100);
                e.Property(x => x.ReceiverPhone).HasMaxLength(30);
                e.Property(x => x.ReceiverAddr).HasMaxLength(255);
                e.HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
                e.HasOne(x => x.Shipment).WithOne(x => x.Order).HasForeignKey<Shipment>(x => x.OrderId);
            });

            builder.Entity<OrderItem>(e =>
            {
                e.HasKey(x => x.OrderItemId);
                e.Property(x => x.UnitPrice).HasPrecision(10, 2);
                e.HasIndex(x => new { x.OrderId, x.BookId }).IsUnique();
            });

            builder.Entity<Shipment>(e =>
            {
                e.HasKey(x => x.ShipmentId);
                e.Property(x => x.Carrier).HasMaxLength(50);
                e.Property(x => x.TrackingNo).HasMaxLength(80);
                e.HasIndex(x => x.OrderId).IsUnique();
            });

            builder.Entity<PurchaseOrder>(e =>
            {
                e.HasKey(x => x.PurchaseOrderId);
                e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);
                e.HasMany(x => x.Items).WithOne(x => x.PurchaseOrder).HasForeignKey(x => x.PurchaseOrderId);
            });

            builder.Entity<PurchaseOrderItem>(e =>
            {
                e.HasKey(x => x.PurchaseOrderItemId);
                e.Property(x => x.UnitCost).HasPrecision(10, 2);
                e.HasIndex(x => new { x.PurchaseOrderId, x.BookId }).IsUnique();
            });

            builder.Entity<StockLedger>(e =>
            {
                e.HasKey(x => x.StockLedgerId);
                e.Property(x => x.Note).HasMaxLength(255);
                e.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId);
                e.HasIndex(x => new { x.BookId, x.CreatedAt });
            });

            builder.Entity<CustomerProfile>(e =>
            {
                e.HasKey(x => x.UserId);
                e.Property(x => x.FullName).HasMaxLength(100);
                e.Property(x => x.Address).HasMaxLength(255);
                e.Property(x => x.AccountBalance).HasPrecision(12, 2);
                e.Property(x => x.OverdraftLimit).HasPrecision(12, 2);
                e.HasOne(x => x.User).WithOne().HasForeignKey<CustomerProfile>(x => x.UserId);
            });

            builder.Entity<OutOfStockRequest>(e =>
            {
                e.HasKey(x => x.OutOfStockRequestId);
                e.Property(x => x.BookTitle).HasMaxLength(200);
                e.Property(x => x.Note).HasMaxLength(500);
                e.Property(x => x.AdminReply).HasMaxLength(500);
                e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
                e.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId);
                e.HasIndex(x => new { x.Status, x.CreatedAt });
            });
        }
    }
}
