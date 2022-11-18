namespace ConsoleOptions;
using System.Reflection;
class ConfigInfo<T>
{
    public Type ConfigType {get;}
    public Queue<PropertyInfo> Required {get;}
    public Dictionary<string, MemberInfo> Optional { get; }
    public bool HasOptional { get; } = false;
    public bool HasCommands { get; } = false;
    
    public ConfigInfo(T configInstance)
    {
        ConfigType = typeof(T);
        Optional = new Dictionary<string, MemberInfo>();
        Required = new Queue<PropertyInfo>();
        
        var props = ConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var methods = ConfigType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        //ignores the property methods for getting/ setting
        methods = methods.Where(m => !m.IsSpecialName).ToArray();

        //members is properties + methods. fancy way of writing &&
        if(props.Length + methods.Length == 0) System.Console.WriteLine("WARNING: Pointless Parser. Options class doesnt have public members");

        bool optionalEncountered = false;
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
                {
                    if(!optionalEncountered)
                        optionalEncountered = true;
                    HasOptional = true;
                }
                else
                {
                    //if requirementsMet is true it means optional param was encountered
                    //this is invalid condition, mandatory parameters should come
                    //first in order of option class properties
                    if(optionalEncountered)
                    {
                        throw new RequiredParamsException($"In options class ({ConfigType}) mandatory params should come before optional params!");
                    }
                }
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
                HasCommands = true;
            }

            var errorAttribute = method.GetCustomAttribute<ParamAttribute>();
            if(errorAttribute is not null) 
                throw new InvalidAttributeException($"You cannot assign a param attribute to methods ({method.Name})! Properties only");
        }
    }
}