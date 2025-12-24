using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Services;

public interface ICartService
{
    IReadOnlyDictionary<long, uint> GetItems();
    void Add(long bookId, uint qty);
    void Set(long bookId, uint qty);
    void Remove(long bookId);
    void Clear();
    int CountItems();
}

public sealed class CartService : ICartService
{
    private const string SessionKey = "Cart";
    private readonly IHttpContextAccessor _http;

    public CartService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public IReadOnlyDictionary<long, uint> GetItems()
    {
        var session = _http.HttpContext?.Session;
        if (session is null)
        {
            return new Dictionary<long, uint>();
        }

        var json = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<long, uint>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<long, uint>>(json) ?? new Dictionary<long, uint>();
        }
        catch
        {
            return new Dictionary<long, uint>();
        }
    }

    public void Add(long bookId, uint qty)
    {
        if (qty == 0) return;
        var cart = GetMutable();
        cart.TryGetValue(bookId, out var existing);
        cart[bookId] = checked(existing + qty);
        Save(cart);
    }

    public void Set(long bookId, uint qty)
    {
        var cart = GetMutable();
        if (qty == 0)
        {
            cart.Remove(bookId);
        }
        else
        {
            cart[bookId] = qty;
        }
        Save(cart);
    }

    public void Remove(long bookId)
    {
        var cart = GetMutable();
        cart.Remove(bookId);
        Save(cart);
    }

    public void Clear()
    {
        var session = _http.HttpContext?.Session;
        session?.Remove(SessionKey);
    }

    public int CountItems()
    {
        var items = GetItems();
        return items.Sum(x => (int)x.Value);
    }

    private Dictionary<long, uint> GetMutable() => new(GetItems());

    private void Save(Dictionary<long, uint> cart)
    {
        var session = _http.HttpContext?.Session;
        if (session is null) return;

        var json = JsonSerializer.Serialize(cart);
        session.SetString(SessionKey, json);
    }
}
