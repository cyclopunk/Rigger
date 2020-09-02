using System;

namespace TheCommons.Forge.Validators
{
    public interface IManagedTypeValidator
    {
        bool Validate(Type type);
    }
}