namespace PostBuildTool
{
    internal class CommandSwitch : ICloneable
    {
        public CommandSwitch(string[] switches, string desc, bool takesArgument, string argumentName, bool required = false, Dictionary<string, string> options = null)
        {
            bool noswitch;
            if (switches == null || switches.Length == 0)
            {
                Switches = new List<string>();
                noswitch = true;
            }
            else
            {
                Switches = new List<string>(switches);
                noswitch = false;
            }

            Description = desc;
            TakesArgument = takesArgument;
            ArgumentName = argumentName;
            Required = required;
            Options = options;

            if (noswitch && ((!TakesArgument) || string.IsNullOrEmpty(argumentName)))
            {
                throw new InvalidOperationException("If you do not specify a switch, you must specify an argument name.");
            }

            if (noswitch)
            {
                ShortName = argumentName;
            }
            else
            {
                Switches.Sort((a, b) =>
                {
                    return a.Length - b.Length;
                });

                ShortName = Switches[0];
            }
        }

        public CommandSwitch(string switches, string desc, string argumentName, bool required = false) : this(switches.Split('|'), desc, true, argumentName, required)
        {
        }

        public CommandSwitch(string switches, string desc, bool required = false) : this(switches.Split('|'), desc, false, null, required)
        {
        }

        public static List<CommandSwitch> Commands { get; set; }
        public static string ProgramTitle { get; } = "post-build-tool";
        public string ArgumentName { get; set; }

        public string ArgumentValue { get; set; }

        public string Description { get; set; }

        public bool IsSet { get; set; }

        public Dictionary<string, string> Options { get; set; }

        public bool Required { get; set; }

        public string ShortName { get; set; }

        public List<string> Switches { get; set; }

        public bool TakesArgument { get; set; }

        public static CommandSwitch FindCommand(string arg)
        {
            arg = arg.Trim().ToLower();
            return Commands.FirstOrDefault(x => x.Switches.Contains(arg));
        }

        public static Dictionary<string, CommandSwitch> ParseArgs(IList<string> args)
        {
            var dict = new Dictionary<string, CommandSwitch>();

            int i, c = args.Count;

            for (i = 0; i < c; i++)
            {
                var arg = args[i];

                var farg = FindCommand(arg);

                if (farg == null)
                {
                    PrintHelp();
                    return default;
                }

                farg = farg.Clone();

                if (arg.StartsWith("/"))
                {
                    if (dict.ContainsKey(farg.ShortName))
                    {
                        Console.WriteLine("Duplicated Command Arguments");
                    }

                    if (i < c - 1 && !args[i + 1].StartsWith("/"))
                    {
                        var dir = args[i + 1];
                        farg.ArgumentValue = dir;
                        i++;
                    }

                    farg.IsSet = true;
                    dict.Add(farg.ShortName, farg);
                }
            }

            return dict;
        }

        public static void PrintHelp(bool exit = true)
        {
            Console.WriteLine("post-build-tool v1.0");
            Console.WriteLine();
            Console.WriteLine("Copyright (C) 2023 Nathaniel Moschkin");
            Console.WriteLine("All Rights Reserved");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            var ptstr = $"    {ProgramTitle} ";
            Console.Write(ptstr);

            int col = ptstr.Length;
            foreach (var cmd in Commands)
            {
                var sc = cmd.ToString();

                if (col + sc.Length > 80)
                {
                    Console.Write("\r\n" + "".PadRight(ptstr.Length));
                    col = ptstr.Length;
                }

                Console.Write(" " + sc);
                col += sc.Length;
            }

            Console.WriteLine();
            Console.WriteLine();

            var maxname = Commands.Max(x => x.Switches.Max(y => y.Length)) + 7;
            var arglen = Commands.Max(x => x.ArgumentName?.Length ?? 0) + 10;

            foreach (var cmd in Commands)
            {
                int i, c = cmd.Switches?.Count ?? 0;
                string prevkey = null;
                int z = 2;

                Console.Write($"    {cmd.Switches[0]}    ".PadRight(maxname));

                if (!string.IsNullOrEmpty(cmd.ArgumentName))
                {
                    Console.Write($"    <{cmd.ArgumentName}> ".PadRight(arglen));
                }
                else
                {
                    Console.Write($"{"".PadRight(arglen)}");
                }

                Console.WriteLine(cmd.Description);

                for (i = 1; i < c; i++)
                {
                    Console.WriteLine($"    {cmd.Switches[i]}");
                }

                if (cmd.Options != null && cmd.Options.Count > 0)
                {
                    prevkey = null;
                    z = 2;

                    foreach (var kv in cmd.Options)
                    {
                        if (kv.Key == prevkey)
                        {
                            Console.Write($"".PadLeft(arglen + maxname));
                            Console.WriteLine(kv.Value);
                            z++;
                        }
                        else
                        {
                            Console.Write($"    {kv.Key}    ".PadLeft(arglen + maxname));
                            Console.WriteLine(kv.Value);
                            z = 2;
                        }

                        prevkey = kv.Key + z.ToString();
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine();

            Console.WriteLine();

            if (exit) Environment.Exit(0);
        }

        public override string ToString()
        {
            if (Switches == null || Switches.Count == 0)
            {
                if (Required)
                {
                    return $"<{ArgumentName}>";
                }
                else
                {
                    return $"[<{ArgumentName}>]";
                }
            }
            else
            {
                var sw = string.Join("|", Switches);
                string arg = "";

                if (!string.IsNullOrEmpty(ArgumentName)) arg = $" <{ArgumentName}>";

                if (Required)
                {
                    return $"{sw}{arg}";
                }
                else
                {
                    return $"[{sw}{arg}]";
                }
            }
        }

        public CommandSwitch Clone()
        {
            return (CommandSwitch)MemberwiseClone();
        }

        object ICloneable.Clone() => Clone();
    }
}