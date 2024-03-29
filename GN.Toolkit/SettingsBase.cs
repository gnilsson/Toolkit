﻿using Microsoft.Extensions.Configuration;

namespace Boolkit;

public abstract class SettingsBase<T> where T : SettingsBase<T>
{
    public T? Bind(IConfiguration configuration)
    {
        configuration.Bind(typeof(T).Name, (T)this);

        return this as T;
    }
}
