        var lookupRows = lookup ? Answers.ToList() : [];
        var captureRows = capture ? Lines.ToList() : [];
        var probeRows = probe ? Dashboard?.ProbeSamples ?? [] : [];
