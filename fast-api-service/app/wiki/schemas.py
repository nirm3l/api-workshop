from pydantic import BaseModel

class WikiRecord(BaseModel):
    wiki: str
    title: str
