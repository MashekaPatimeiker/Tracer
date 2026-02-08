using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization
{
    public static class SerializerLoader
    {
        public static List<ITraceResultSerializer> LoadSerializers(string? pluginsPath = null)
        {
            pluginsPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            
            Console.WriteLine($"Loading plugins from: {pluginsPath}");
            
            if (!Directory.Exists(pluginsPath))
            {
                Console.WriteLine($"Plugins directory does not exist: {pluginsPath}");
                return new List<ITraceResultSerializer>();
            }

            var serializers = new List<ITraceResultSerializer>();
            var assemblyFiles = Directory.GetFiles(pluginsPath, "*.dll");

            Console.WriteLine($"Found {assemblyFiles.Length} DLL files");

            foreach (var assemblyFile in assemblyFiles)
            {
                Console.WriteLine($"Processing: {Path.GetFileName(assemblyFile)}");
                
                try
                {
                    // Загружаем сборку через LoadFrom с помощью AssemblyLoadContext
                    var assembly = Assembly.LoadFrom(assemblyFile);
                    Console.WriteLine($"Loaded assembly: {assembly.FullName}");
                    
                    var serializerTypes = assembly.GetTypes()
                        .Where(t => typeof(ITraceResultSerializer).IsAssignableFrom(t) 
                                 && !t.IsInterface 
                                 && !t.IsAbstract);

                    foreach (var type in serializerTypes)
                    {
                        Console.WriteLine($"Found serializer type: {type.FullName}");
                        
                        try
                        {
                            var serializer = Activator.CreateInstance(type) as ITraceResultSerializer;
                            if (serializer != null)
                            {
                                serializers.Add(serializer);
                                Console.WriteLine($"✓ Successfully loaded serializer: {serializer.GetType().Name} (Format: {serializer.Format})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"✗ Failed to create instance of {type.Name}: {ex.Message}");
                            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Failed to load assembly {Path.GetFileName(assemblyFile)}: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                }
            }

            Console.WriteLine($"Total loaded serializers: {serializers.Count}");
            return serializers;
        }
    }
}