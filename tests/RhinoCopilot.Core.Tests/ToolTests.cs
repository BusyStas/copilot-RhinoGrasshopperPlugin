using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Model;
using RhinoCopilot.Core.Tests.Support;
using RhinoCopilot.Core.Tools;
using Xunit;

namespace RhinoCopilot.Core.Tests
{
    public class ToolTests
    {
        private readonly FakeModelAdapter _model = new FakeModelAdapter();

        [Fact]
        public void Every_tool_has_a_valid_name_description_and_object_schema()
        {
            ToolRegistry registry = DefaultTools.CreateRegistry(_model);

            Assert.NotEmpty(registry.Tools);
            foreach (ITool tool in registry.Tools)
            {
                Assert.Matches(new Regex("^[a-zA-Z0-9_-]{1,64}$"), tool.Name);
                Assert.False(string.IsNullOrWhiteSpace(tool.Description));
                Assert.Equal("object", (string)tool.InputSchema["type"]);
            }
        }

        [Fact]
        public void Registry_rejects_duplicate_names()
        {
            var registry = new ToolRegistry();
            registry.Register(new StubTool("same"));

            Assert.Throws<InvalidOperationException>(() => registry.Register(new StubTool("same")));
        }

        [Fact]
        public void Registry_definitions_match_the_messages_api_shape()
        {
            JArray definitions = DefaultTools.CreateRegistry(_model).GetDefinitions();

            foreach (JToken definition in definitions)
            {
                Assert.NotNull(definition["name"]);
                Assert.NotNull(definition["description"]);
                Assert.Equal("object", (string)definition["input_schema"]["type"]);
            }
        }

        [Fact]
        public void Create_box_passes_defaults_and_returns_the_new_id()
        {
            var tool = new CreateBoxTool(_model);

            ToolResult result = tool.Execute(new JObject { ["size_x"] = 2, ["size_y"] = 3, ["size_z"] = 4 });

            Assert.False(result.IsError);
            Assert.Equal("box-1", (string)JObject.Parse(result.Content)["id"]);
            Assert.Equal("0,0,0,2,3,4,(none)", _model.BoxCalls[0]);
        }

        [Fact]
        public void Create_box_passes_position_and_layer()
        {
            var tool = new CreateBoxTool(_model);

            tool.Execute(new JObject { ["x"] = 1, ["y"] = 2, ["z"] = 3, ["size_x"] = 5, ["size_y"] = 5, ["size_z"] = 5, ["layer"] = "Walls" });

            Assert.Equal("1,2,3,5,5,5,Walls", _model.BoxCalls[0]);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_box_rejects_non_positive_sizes(int size)
        {
            var tool = new CreateBoxTool(_model);

            Assert.Throws<ArgumentException>(() =>
                tool.Execute(new JObject { ["size_x"] = size, ["size_y"] = 1, ["size_z"] = 1 }));
            Assert.Empty(_model.BoxCalls);
        }

        [Fact]
        public void Create_box_requires_every_size()
        {
            var tool = new CreateBoxTool(_model);

            Assert.Throws<ArgumentException>(() => tool.Execute(new JObject { ["size_x"] = 1, ["size_y"] = 1 }));
        }

        [Fact]
        public void Create_box_rejects_a_non_numeric_size()
        {
            var tool = new CreateBoxTool(_model);

            Assert.Throws<ArgumentException>(() =>
                tool.Execute(new JObject { ["size_x"] = "big", ["size_y"] = 1, ["size_z"] = 1 }));
        }

        [Fact]
        public void Get_selection_reports_the_total_and_truncates_long_lists()
        {
            for (int i = 0; i < GetSelectionTool.MaxObjects + 50; i++)
            {
                _model.Selection.Add(new ObjectSummary { Id = "id" + i, Type = "Brep", Layer = "Default", Name = "" });
            }

            ToolResult result = new GetSelectionTool(_model).Execute(new JObject());

            JObject json = JObject.Parse(result.Content);
            Assert.Equal(GetSelectionTool.MaxObjects + 50, (int)json["count"]);
            Assert.True((bool)json["truncated"]);
            Assert.Equal(GetSelectionTool.MaxObjects, ((JArray)json["objects"]).Count);
        }

        [Fact]
        public void List_layers_returns_every_layer()
        {
            ToolResult result = new ListLayersTool(_model).Execute(new JObject());

            JObject json = JObject.Parse(result.Content);
            Assert.Equal(1, (int)json["count"]);
            Assert.Equal("Default", (string)json["layers"][0]["Path"]);
        }

        [Fact]
        public void Get_document_info_includes_the_units()
        {
            ToolResult result = new GetDocumentInfoTool(_model).Execute(new JObject());

            Assert.Equal("Millimeters", (string)JObject.Parse(result.Content)["Units"]);
        }
    }
}
