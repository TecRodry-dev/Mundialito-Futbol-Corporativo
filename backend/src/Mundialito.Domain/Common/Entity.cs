namespace Mundialito.Domain.Common;

/// <summary>
/// Base de todas las entidades: cada una se identifica por su Id.
/// </summary>
public abstract class Entity
{
    protected Entity(Guid id)
    {
        Id = id;
    }

    // Constructor vacío que usa EF Core para reconstruir la entidad desde la base de datos.
    protected Entity()
    {
    }

    public Guid Id { get; private set; }
}
