using Microsoft.AspNetCore.Mvc;
using RecipesRepacoSayco.Plc.Managers;
namespace RecipesRepacoSayco.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlcController : ControllerBase
{
    private readonly PlcManager _manager;

    public PlcController(PlcManager manager)
    {
        _manager = manager;
    }

    [HttpGet("{plcName}/tags")]
    public IActionResult GetTags(string plcName)
    {
        var tags = _manager.GetTags(plcName);
        if (tags == null)
            return NotFound($"PLC '{plcName}' not found.");
        return Ok(tags.Select(t => new
        {
            t.Name,
            t.Address,
            t.Datatype,
            t.Value
        }));
    }

    [HttpPost("{plcName}/start")]
    public async Task<IActionResult> StartPlc(string plcName)
    {
        await _manager.StartPlcAsync(plcName);
        return Ok($"PLC '{plcName}' started.");
    }

    [HttpPost("{plcName}/stop")]
    public async Task<IActionResult> StopPlc(string plcName)
    {
        await _manager.StopPlcAsync(plcName);
        return Ok($"PLC '{plcName}' stopped.");
    }

    [HttpPost("{plcName}/write")]
    public async Task<IActionResult> WriteTag(string plcName, [FromBody] TagWriteRequest request)
    {
        var result = await _manager.WriteTagAsync(plcName, request.TagName, request.Value);
        return result ? Ok("Value written.") : BadRequest("Write failed.");
    }
}

public class TagWriteRequest
{
    public string TagName { get; set; }
    public object Value { get; set; }
}
