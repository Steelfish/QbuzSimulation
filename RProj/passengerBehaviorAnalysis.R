#Fitting a distribution for the amount of passengers per hour.
#This average per hour can then be used as the rate parameter to generate
#interarrival times via a Poisson proces.


processBusData <- function(busData)
{
    # Remove unnecessary Trip information.
    busData <- subset(busData, select=-Trip)
    
    # Sum all passenger arrivals within an hour per day.
    busData <- aggregate(x = busData[colnames(busData[,3:ncol(busData)])],
                         FUN = sum,
                         by = list(Date = busData$Date,
                                   Time = as.POSIXlt(strptime(busData$Departure, format="%H:%M"))$hour))
    
    # https://stackoverflow.com/questions/13649019/with-r-split-time-series-data-into-time-intervals-say-an-hour-and-then-plot-t
    
    
    # First sort by time and by date to have an ordering of days and their hours.
    # https://stackoverflow.com/questions/10683224/obtain-hour-from-datetime-vector
    #busData <- busData[order(as.Date(busData$Date, format="%d-%m-%Y"),
    #                                 as.POSIXlt(busData$Time, format="%H:%M", origin="1970-01-01")),
    #                                 drop=FALSE]
    
    # Remove unnecessary row names.
    row.names(busData) <- NULL
    
    # Sum all passenger arrivals within an hour per day.
    busData <- aggregate(x = busData[colnames(busData[,3:ncol(busData)])],
                         FUN = mean,
                         by = list(Time = busData$Time))
    
    return(busData)
}


# The bus data going from AZU, Heidelberglaan, De Kromme Rijn, Stadion Galgenwaard,
#                         Rubenslaan, Sterrenwijk, Bleekstraat to CS Centrumzijde.
arrivals_AZU_Centraal <- read.csv("12a_entering.csv",
                                  header=TRUE, sep=";")

# The bus data going from CS Centrumzijde, Bleekstraat, Sterrenwijk, Rubenslaan,
#                         Stadion Galgenwaard, De Kromme Rijn, Heidelberglaan to AZU.
arrivals_Centraal_AZU <- read.csv("12b_entering.csv",
                                  header=TRUE, sep=";")


arrivals_AZU_Centraal <- processBusData(arrivals_AZU_Centraal)
arrivals_Centraal_AZU <- processBusData(arrivals_Centraal_AZU)



#Fitting a distribution for the amount of passengers per hour.
#This average per hour can then be used as the probability of having a stop as a destination.

# The bus data going from AZU, Heidelberglaan, De Kromme Rijn, Stadion Galgenwaard,
#                         Rubenslaan, Sterrenwijk, Bleekstraat to CS Centrumzijde.
destinations_AZU_Centraal <- read.csv("12a_leaving.csv",
                                      header=TRUE, sep=";")

# The bus data going from CS Centrumzijde, Bleekstraat, Sterrenwijk, Rubenslaan,
#                         Stadion Galgenwaard, De Kromme Rijn, Heidelberglaan to AZU.
destinations_Centraal_AZU <- read.csv("12b_leaving.csv",
                                      header=TRUE, sep=";")


destinations_AZU_Centraal <- processBusData(destinations_AZU_Centraal)
destinations_Centraal_AZU <- processBusData(destinations_Centraal_AZU)



# Scale the entry and exit amounts by the tram passenger prognosis.
prognosis_AZU_Centraal <- read.csv("passengerprognosea.csv",
                                   header=TRUE, sep=";")

prognosis_Centraal_AZU <- read.csv("passengerprognoseb.csv",
                                   header=TRUE, sep=";")

