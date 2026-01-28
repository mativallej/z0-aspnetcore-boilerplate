namespace Z0.Domain.Entities;

public class Item : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Item() { }

    public Item(string name, string? description = null)
    {
        SetName(name);
        Description = description;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        Name = name;
        SetUpdatedAt();
    }

    public void SetDescription(string? description)
    {
        Description = description;
        SetUpdatedAt();
    }

    public void Update(string name, string? description)
    {
        SetName(name);
        SetDescription(description);
    }
}
