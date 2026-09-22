namespace AzureTest.Contracts.DTOs.Companies;

public interface ICompanyFormDto
{
    public string CompanyName { get; }

    public string? KvkNumber { get;  }

    public string? Location { get;  }

    public bool IsINDMember { get;  }
}