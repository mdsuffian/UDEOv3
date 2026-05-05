# Expert Plugin Template
# Save this file in the plugins/ directory with a descriptive name.
# It will be auto-discovered and registered on UDEO startup.
#
# Required: the file must call Register-UDEOExpert with at minimum:
#   -Id, -Name, -Type, -Execute
#
# The $Context parameter is a UDEOContext object (see engine/UDEO.Engine.psm1).
# Return a hashtable with: Success, DecisionCode, RuleFired

Register-UDEOExpert -Id 'my.custom_expert' `
    -Name 'My Custom Expert' `
    -Type Custom `
    -Description 'A custom expert that does something useful.' `
    -Execute {
        param($Context, $Parameters)

        # Access context data
        # $Context.Data['some_key']

        # Access parameters
        # $Parameters['some_param']

        # Your logic here
        $threshold = if ($Parameters.ContainsKey('threshold')) { [double]$Parameters['threshold'] } else { 0.5 }
        $value = [double]$Context.Data['value']

        if ($value -gt $threshold) {
            return @{
                Success      = $true
                DecisionCode = 'APPROVED'
                RuleFired    = "VALUE_ABOVE_THRESHOLD:value=$value > threshold=$threshold"
            }
        } else {
            return @{
                Success      = $true
                DecisionCode = 'REJECTED'
                RuleFired    = "VALUE_BELOW_THRESHOLD:value=$value <= threshold=$threshold"
            }
        }
    } `
    -HealthCheck {
        # Return $true if expert is healthy, $false otherwise
        return $true
    }

Write-Host "  Custom expert plugin loaded: my.custom_expert" -ForegroundColor DarkGray
