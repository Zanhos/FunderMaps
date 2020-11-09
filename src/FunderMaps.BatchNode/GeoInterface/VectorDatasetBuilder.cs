using System;
using System.Collections.Generic;
using System.IO;
using FunderMaps.BatchNode.Command;
using FunderMaps.Core.Types;
using Npgsql;

namespace FunderMaps.BatchNode.GeoInterface
{
    public abstract class DataSource
    {
        public GeometryExportFormat Format { get; set; }

        public virtual string Read(CommandInfo commandInfo)
        {
            return ToString();
        }

        public virtual string Write(CommandInfo commandInfo)
        {
            return ToString();
        }
    }

    public class LayerSource
    {
        public IEnumerable<string> Layers { get; set; }

        public virtual void Imbue(CommandInfo commandInfo)
        {
            if (Layers != null)
            {
                foreach (var item in Layers)
                {
                    commandInfo.ArgumentList.Add(item.Trim());
                }
            }
        }

        public static LayerSource Enumerable(IEnumerable<string> layers)
            => new LayerSource
            {
                Layers = layers,
            };
    }

    public class SqlLayerSource : LayerSource
    {
        public string Query { get; set; }

        public override void Imbue(CommandInfo commandInfo)
        {
            if (Query != null)
            {
                commandInfo.ArgumentList.Add("-sql");
                commandInfo.ArgumentList.Add(Query.Replace('\n', ' ').Trim());
            }
        }

        public static SqlLayerSource FromSql(string query)
            => new SqlLayerSource
            {
                Query = query,
            };
    }

    public class PostreSQLDataSource : DataSource
    {
        // FUTURE: Replace with non npgsql version
        private NpgsqlConnectionStringBuilder connectionStringBuilder;

        public PostreSQLDataSource(string dbConnection)
        {
            connectionStringBuilder = new NpgsqlConnectionStringBuilder(dbConnection);
        }

        public override string Read(CommandInfo commandInfo)
        {
            commandInfo.Environment.Add("PGHOST", connectionStringBuilder.Host);
            commandInfo.Environment.Add("PGUSER", connectionStringBuilder.Username);
            commandInfo.Environment.Add("PGPASSWORD", connectionStringBuilder.Password);

            return $"PG:dbname={connectionStringBuilder.Database}";
        }

        public override string Write(CommandInfo commandInfo)
        {
            commandInfo.Environment.Add("PGHOST", connectionStringBuilder.Host);
            commandInfo.Environment.Add("PGUSER", connectionStringBuilder.Username);
            commandInfo.Environment.Add("PGPASSWORD", connectionStringBuilder.Password);

            return $"PG:dbname={connectionStringBuilder.Database}";
        }

        public static PostreSQLDataSource FromConnectionString(string connectionString)
            => new PostreSQLDataSource(connectionString);
    }

    public class FileDataSource : DataSource
    {
        public string PathPrefix { get; set; }
        public string Name { get; set; }
        public string Extension => VectorDatasetBuilder.ExportFormatTuple(Format).Item2;
        public string FileName => $"{Name}{Extension}";

        public override string ToString()
        {
            return !string.IsNullOrEmpty(PathPrefix) ? Path.Combine(PathPrefix, FileName) : FileName;
        }
    }

    public class VectorDatasetBuilder
    {
        private const string CommandName = "ogr2ogr";

        private DataSource input;
        private DataSource output;
        private LayerSource inputLayer = new LayerSource();

        private VectorDatasetBuilderOptions _options;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public VectorDatasetBuilder(VectorDatasetBuilderOptions options = null)
        {
            _options = options ?? new VectorDatasetBuilderOptions();
        }

        public VectorDatasetBuilder InputDataset(DataSource input)
        {
            this.input = input;
            return this;
        }

        public VectorDatasetBuilder OutputDataset(DataSource output)
        {
            this.output = output;
            return this;
        }

        public VectorDatasetBuilder InputLayers(LayerSource input)
        {
            inputLayer = input ?? new LayerSource();
            return this;
        }

        public static (string, string) ExportFormatTuple(GeometryExportFormat format)
            => format switch
            {
                GeometryExportFormat.MapboxVectorTiles => ("MVT", ""),
                GeometryExportFormat.GeoPackage => ("GPKG", ".gpkg"),
                GeometryExportFormat.ESRIShapefile => ("ESRI Shapefile", ".shp"),
                GeometryExportFormat.GeoJSON => ("GeoJSON", ".json"),
                _ => throw new InvalidOperationException(nameof(format)),
            };

        /// <summary>
        ///     Build the command from builder parts.
        /// </summary>
        /// <returns>The <see cref="CommandInfo"/> to be executed.</returns>
        public CommandInfo Build()
        {
            // NOTE: Keep the order in which arguments needs to be passed to the command.
            var command = new CommandInfo(CommandName);
            command.ArgumentList.Add("-overwrite");
            command.ArgumentList.Add("-f");
            command.ArgumentList.Add(ExportFormatTuple(output.Format).Item1);
            command.ArgumentList.Add(output.Write(command));
            command.ArgumentList.Add(input.Read(command));

            inputLayer.Imbue(command);

            if (!string.IsNullOrEmpty(_options.AdditionalOptions))
            {
                foreach (var argument in _options.AdditionalOptions.Split(" "))
                {
                    command.ArgumentList.Add(argument.Trim());
                }
            }

            return command;
        }
    }
}
