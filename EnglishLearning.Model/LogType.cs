﻿using System;

namespace EnglishLearning.Model
{
    [Flags]
    public enum LogType : int
    {
        None = 0,
        Info = 2,
        Error = 4
    }
}
