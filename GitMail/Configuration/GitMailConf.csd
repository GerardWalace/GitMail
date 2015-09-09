<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="c5f6cfa3-856e-4533-83e4-0da973fb5c4a" namespace="GitMail.Configuration" xmlSchemaNamespace="urn:GitMail.Configuration" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationElementCollection name="RepositoryCollection" xmlItemName="repositoryConfiguration" codeGenOptions="Indexer, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/RepositoryConfiguration" />
      </itemType>
    </configurationElementCollection>
    <configurationSection name="GitMailConfiguration" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="gitMailConfiguration">
      <attributeProperties>
        <attributeProperty name="GitPath" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="gitPath" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Repositories" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="repositories" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/RepositoryCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="RepositoryConfiguration">
      <attributeProperties>
        <attributeProperty name="RepositoryPath" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="repositoryPath" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="DirectoryPath" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="directoryPath" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Merges" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="merges" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/MergeCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="MergeCollection" xmlItemName="mergeConfiguration" codeGenOptions="Indexer, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/MergeConfiguration" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="MergeConfiguration">
      <attributeProperties>
        <attributeProperty name="IntoBranch" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="intoBranch" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="FromBranch" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="fromBranch" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MailsDesReferents" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="mailsDesReferents" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MailObject" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="mailObject" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/c5f6cfa3-856e-4533-83e4-0da973fb5c4a/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>