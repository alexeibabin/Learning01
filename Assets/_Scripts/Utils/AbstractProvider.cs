using UniRx;

public class AbstractProvider<TType, TProduct>
{
    private readonly ReactiveDictionary<TType, TProduct> _products = new ReactiveDictionary<TType, TProduct>();

    public IReadOnlyReactiveDictionary<TType, TProduct> Products => _products;

    public void Register(TType type, TProduct product)
    {
        if (_products.ContainsKey(type) == false)
            _products.Add(type, product);
    }

    public bool TryGet(TType type, out TProduct product)
    {
        return _products.TryGetValue(type, out product);
    }
}