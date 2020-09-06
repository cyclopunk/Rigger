using System;

namespace Rigger.Validators
{
    public interface IComponentValidator
    {
        bool Validate(Type type);
    }
}