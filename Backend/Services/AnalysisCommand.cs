namespace Backend.Services;

public interface ICommand
{
    void Execute();
    void Undo();
}

public class AddProductCommand : ICommand
{
    private readonly List<Models.Product> _list;
    private readonly Models.Product _product;

    public AddProductCommand(List<Models.Product> list, Models.Product product)
    {
        _list = list;
        _product = product;
    }

    public void Execute() => _list.Add(_product);
    public void Undo() => _list.Remove(_product);
}