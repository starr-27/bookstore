using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Entities;

namespace WebApplication1.Data;

public static class SeedData
{
    public static async Task EnsureSeedBooksAsync(ApplicationDbContext db)
    {
        // Only do a light seed to avoid overwriting user's real data.
        // If there are already books, skip.
        if (await db.Books.AsNoTracking().AnyAsync())
        {
            return;
        }

        var categoryTech = await GetOrCreateCategoryAsync(db, "计算机/互联网");
        var categoryLit = await GetOrCreateCategoryAsync(db, "文学");
        var categoryChildren = await GetOrCreateCategoryAsync(db, "少儿");

        var pubRm = await GetOrCreatePublisherAsync(db, "人民邮电出版社");
        var pubIct = await GetOrCreatePublisherAsync(db, "清华大学出版社");
        var pubCittic = await GetOrCreatePublisherAsync(db, "中信出版社");

        var supplier = await db.Suppliers.AsNoTracking().OrderBy(s => s.SupplierId).FirstOrDefaultAsync();
        var supplierId = supplier?.SupplierId;

        var now = DateTime.UtcNow;

        var books = new List<Book>
        {
            new()
            {
                BookNo = "CN-100001",
                Title = "深入浅出 ASP.NET Core（第2版）",
                VolumeNo = null,
                CategoryId = categoryTech.CategoryId,
                PublisherId = pubRm.PublisherId,
                SupplierId = supplierId,
                SupplierCatalogEnabled = true,
                Price = 88.00m,
                StockQty = 30,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new()
            {
                BookNo = "CN-100002",
                Title = "C# 从入门到实战",
                VolumeNo = null,
                CategoryId = categoryTech.CategoryId,
                PublisherId = pubIct.PublisherId,
                SupplierId = supplierId,
                SupplierCatalogEnabled = true,
                Price = 79.00m,
                StockQty = 42,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new()
            {
                BookNo = "CN-200001",
                Title = "活着",
                VolumeNo = null,
                CategoryId = categoryLit.CategoryId,
                PublisherId = pubCittic.PublisherId,
                SupplierId = supplierId,
                SupplierCatalogEnabled = true,
                Price = 39.00m,
                StockQty = 55,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new()
            {
                BookNo = "CN-300001",
                Title = "小王子",
                VolumeNo = null,
                CategoryId = categoryChildren.CategoryId,
                PublisherId = pubCittic.PublisherId,
                SupplierId = supplierId,
                SupplierCatalogEnabled = true,
                Price = 28.00m,
                StockQty = 60,
                CreatedAt = now,
                UpdatedAt = now,
            },
        };

        db.Books.AddRange(books);
        await db.SaveChangesAsync();

        await EnsureAuthorsAndKeywordsAsync(db, books[0],
            authors: ["张三"],
            keywords: ["ASP.NET Core", "Web", "后端"]);

        await EnsureAuthorsAndKeywordsAsync(db, books[1],
            authors: ["李四"],
            keywords: ["C#", ".NET", "编程"]);

        await EnsureAuthorsAndKeywordsAsync(db, books[2],
            authors: ["余华"],
            keywords: ["小说", "文学"]);

        await EnsureAuthorsAndKeywordsAsync(db, books[3],
            authors: ["安托万·德·圣埃克苏佩里"],
            keywords: ["童话", "成长"]);
    }

    private static async Task<Category> GetOrCreateCategoryAsync(ApplicationDbContext db, string name)
    {
        var existing = await db.Categories.FirstOrDefaultAsync(x => x.CategoryName == name);
        if (existing is not null)
        {
            return existing;
        }

        var entity = new Category { CategoryName = name };
        db.Categories.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    private static async Task<Publisher> GetOrCreatePublisherAsync(ApplicationDbContext db, string name)
    {
        var existing = await db.Publishers.FirstOrDefaultAsync(x => x.PublisherName == name);
        if (existing is not null)
        {
            return existing;
        }

        var entity = new Publisher { PublisherName = name };
        db.Publishers.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    private static async Task<Author> GetOrCreateAuthorAsync(ApplicationDbContext db, string name)
    {
        var existing = await db.Authors.FirstOrDefaultAsync(x => x.AuthorName == name);
        if (existing is not null)
        {
            return existing;
        }

        var entity = new Author { AuthorName = name };
        db.Authors.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    private static async Task<Keyword> GetOrCreateKeywordAsync(ApplicationDbContext db, string text)
    {
        var existing = await db.Keywords.FirstOrDefaultAsync(x => x.KeywordText == text);
        if (existing is not null)
        {
            return existing;
        }

        var entity = new Keyword { KeywordText = text };
        db.Keywords.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    private static async Task EnsureAuthorsAndKeywordsAsync(ApplicationDbContext db, Book book, string[] authors, string[] keywords)
    {
        // Authors
        for (byte i = 0; i < authors.Length; i++)
        {
            var a = await GetOrCreateAuthorAsync(db, authors[i]);
            var exists = await db.BookAuthors.AnyAsync(x => x.BookId == book.BookId && x.AuthorId == a.AuthorId);
            if (!exists)
            {
                db.BookAuthors.Add(new BookAuthor { BookId = book.BookId, AuthorId = a.AuthorId, AuthorOrder = i });
            }
        }

        // Keywords
        foreach (var k in keywords)
        {
            var kw = await GetOrCreateKeywordAsync(db, k);
            var exists = await db.BookKeywords.AnyAsync(x => x.BookId == book.BookId && x.KeywordId == kw.KeywordId);
            if (!exists)
            {
                db.BookKeywords.Add(new BookKeyword { BookId = book.BookId, KeywordId = kw.KeywordId });
            }
        }

        await db.SaveChangesAsync();
    }
}
