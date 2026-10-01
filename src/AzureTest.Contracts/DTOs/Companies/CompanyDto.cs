

using System;

namespace AzureTest.Contracts.DTOs.Companies;

public record CompanyDto(Guid Id, string CompanyName, string? KvkNumber, bool IsINDMember,string? Location);
public sealed record CreateCompanyDto(string CompanyName, string? KvkNumber, bool IsINDMember,string? Location) :ICompanyFormDto;
public record UpdateCompanyDto(Guid Id, string CompanyName, string? KvkNumber, bool IsINDMember,string? Location):ICompanyFormDto;
