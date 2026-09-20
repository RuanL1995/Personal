public static class VacationPlannerPrompt
{
    public const string SystemPrompt = "You are a vacation planner. Return JSON only. Schema: [{action:string, params:object}]. Example: [{\"action\":\"searchFlights\",\"params\":{\"origin\":\"JNB\",\"destination\":\"CPT\",\"departDate\":\"2026-10-01\"}}]";
    public const string UserPrompt = "Collect name,email,origin,destination,departDate,returnDate,budget,preferences and create a sequential vacation plan.";
    public static readonly (string User, string Assistant)[] FewShotExamples =
    {
        ("Cheap JNB-CPT vacation", "[{\"action\":\"searchFlights\",\"params\":{\"origin\":\"JNB\",\"destination\":\"CPT\",\"departDate\":\"2026-10-01\"}},{\"action\":\"compareOptions\",\"params\":{}},{\"action\":\"selectItinerary\",\"params\":{}},{\"action\":\"bookApi\",\"params\":{}},{\"action\":\"postDecision\",\"params\":{}}]"),
        ("Business JNB-LHR vacation, fastest criteria", "[{\"action\":\"searchFlights\",\"params\":{\"origin\":\"JNB\",\"destination\":\"LHR\",\"departDate\":\"2026-10-01\"}},{\"action\":\"compareOptions\",\"params\":{\"criteria\":\"fastest\"}},{\"action\":\"selectItinerary\",\"params\":{\"criteria\":\"fastest\"}},{\"action\":\"bookApi\",\"params\":{}},{\"action\":\"postDecision\",\"params\":{}}]")
    };
}
