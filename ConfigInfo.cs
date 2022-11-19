namespace ConsoleOptions;
using System.Reflection;
class ConfigInfo<T>
{
    public Type ConfigType {get;}
    public Queue<PropertyInfo> RequiredQueue {get;}
    public Dictionary<string, MemberInfo> OptionsTable { get; }
    
    public ConfigInfo(T configInstance)
    {
        ConfigType = typeof(T);
        OptionsTable = new Dictionary<string, MemberInfo>();
        RequiredQueue = new Queue<PropertyInfo>();
        
        var props = ConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var methods = ConfigType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        //ignores the property methods for getting/ setting
        methods = methods.Where(m => !m.IsSpecialName).ToArray();

        //members is properties + methods. fancy way of writing &&
        if(props.Length + methods.Length == 0) System.Console.WriteLine("WARNING: Pointless Parser. Options class doesnt have public members");

        foreach (var prop in props)
        {
            var param = prop.GetCustomAttribute<ParamAttribute>();

            if(param is not null)
            {
                if(prop.PropertyType != typeof(string))
                    throw new InvalidAttributeException($"Console params should all be of type string, while {prop.Name} is {prop.PropertyType}!");

                if(prop.GetSetMethod() is null)
                    throw new InvalidAttributeException($"No public set method for property: {prop.Name}, cannot be a console parameter!");

                if(param.Optional)
                    OptionsTable.Add(param.Name, prop);
                else
                    RequiredQueue.Enqueue(prop);
            }

            var errorAttribute = prop.GetCustomAttribute<CommandAttribute>();
            if(errorAttribute is not null) 
                throw new InvalidAttributeException($"You cannot assign a command attribute to properties ({prop.Name})! Methods only");
        }
        

        foreach (var method in methods)
        {
            var command = method.GetCustomAttribute<CommandAttribute>();
            if(command is not null)
            {
                if(method.GetParameters().Length > 0)
                    throw new InvalidAttributeException($"Methods that are called by cmd commands cannot have parameters! {method.Name} has parameters!");
                if(method.ReturnType != typeof(void))
                    throw new InvalidAttributeException("Methods that are marked by the command attribute should not have returned type.");
                
                OptionsTable.Add(command.Name, method);
            }

            var errorAttribute = method.GetCustomAttribute<ParamAttribute>();
            if(errorAttribute is not null) 
                throw new InvalidAttributeException($"You cannot assign a param attribute to methods ({method.Name})! Properties only");
        }
    }
}