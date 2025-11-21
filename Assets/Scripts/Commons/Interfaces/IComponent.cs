// -------------------- //
// Script/Commons/Interfaces/IComponent.cs
// -------------------- //

public interface IValidateable
{
    public void Validate();
}

public interface ICloneable<T> 
{
    public T Clone();
}

public interface IConfig: IValidateable, ICloneable<IConfig> { }

public interface IModel: IValidateable, ICloneable<IModel> { }