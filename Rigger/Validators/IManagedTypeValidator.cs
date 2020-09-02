using System;

namespace Rigger.Validators
{
    public interface IManagedTypeValidator
    {
        bool Validate(Type type);
    }
}