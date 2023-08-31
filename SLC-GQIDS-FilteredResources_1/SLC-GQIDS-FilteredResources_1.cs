using System;
using System.Collections.Generic;
using System.Linq;

using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.SLDataGateway;

[GQIMetaData(Name = "Filtered Resources")]
public class MyDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
{
    private readonly GQIStringArgument _resourcePoolArg = new GQIStringArgument("Resource Pool") { IsRequired = false, DefaultValue = string.Empty };

    private GQIDMS _dms;
    private string _resourcePool;
    private List<GQIColumn> _columns;

    public GQIColumn[] GetColumns()
    {
        return _columns.ToArray();
    }

    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { _resourcePoolArg };
    }

    public GQIPage GetNextPage(GetNextPageInputArgs args)
    {
        FilterElement<Resource> filter;
        var resourcePoolFilter = ResourceExposers.PoolGUIDs.Contains(new Guid(_resourcePool));
        filter = new ANDFilterElement<Resource>(resourcePoolFilter);

        ResourceResponseMessage resources = GetResources(filter);

        var rows = GenerateRows(resources);

        var page = new GQIPage(rows.ToArray())
        {
            HasNextPage = false,
        };

        return page;
    }

    private List<GQIRow> GenerateRows(ResourceResponseMessage resources)
    {
        List<GQIRow> rows = new List<GQIRow>();
        foreach (var resource in resources.ResourceManagerObjects)
        {
            List<GQICell> cells = new List<GQICell>();

            foreach (var column in _columns)
            {
                switch (column.Name)
                {
                    case "ID":
                        {
                            cells.Add(new GQICell() { Value = resource.ID.ToString() });
                            break;
                        }

                    case "Name":
                        {
                            cells.Add(new GQICell() { Value = resource.Name });
                            break;
                        }
                }
            }

            rows.Add(new GQIRow(cells.ToArray()));
        }

        return rows;
    }

    private ResourceResponseMessage GetResources(FilterElement<Resource> filter)
    {
        ResourceResponseMessage resourceResponse;
        resourceResponse = (ResourceResponseMessage)_dms.SendMessage(new GetResourceMessage(filter));
        return resourceResponse;
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _columns = new List<GQIColumn>
        {
         new GQIStringColumn("ID"),
         new GQIStringColumn("Name"),
        };

        _resourcePool = args.GetArgumentValue(_resourcePoolArg);

        return new OnArgumentsProcessedOutputArgs();
    }

    public OnInitOutputArgs OnInit(OnInitInputArgs args)
    {
        _dms = args.DMS;
        return new OnInitOutputArgs();
    }
}