import axios from 'axios'


const api = axios.create({
  baseURL: 'http://localhost:5266/api/v1/ReportGeneration',
})

export const getCompanies = async () => {
  try {
    const response = await api.get('/all-companies')
    return response.data
  } catch (error) {
    console.error(error)
    return []
  }
};


export const reportGeneration = async ({prompt, companyId, company_name}) => {

  try {
    const response = await api.post('/report-generator', {
      "sessionId": "1",
      "useId": "1",
      "prompt": prompt,
      "CompanyId": companyId,
      "CompanyName": company_name
    })
    return response.data
  } catch (error) {
    console.error(error)
    return []
  }
}