module cvParser.JsonParse

    type Person = {
      name: Name
      contactInformation: ContactInformation
      languages: list<Language>
      summary: string
      education: list<Education>
      workExperience: list<WorkExperience>
      skills: list<string>
      personalQualifications: list<string>
    }
    
    and Name = {
      firstName: string
      lastName: string
      middleName: option<string>  // Use option type for optional middle name
    }
    
    and ContactInformation = {
      address: Address
      phone: string
      email: string
    }
    
    and Address = {
      city: string
      postalCode: string
    }
    
    and Language = {
      language: string
      fluency: string
    }
    
    and Education = {
      institution: string
      degree: option<string>       // Use option type for optional degree
      fieldOfStudy: option<string> // Use option type for optional fieldOf study
      startDate: string
      endDate: string
    }
    
    and WorkExperience = {
      company: string
      title: string
      startDate: string
      endDate: string
      description: string
    }
    
    open System.Text
    
    let cvData = System.IO.File.ReadAllBytes("/src/cvParser/CV.json")
    let data = Json.JsonDocument.Parse(cvData)
