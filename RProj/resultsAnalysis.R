##########################################
plotNormalDistribution <- function(x, mean, sd, xlabel, subject) {
    # Plot the histogram with normal density curve.
    # Source: https://www.r-bloggers.com/exploratory-data-analysis-combining-histograms-and-density-plots-to-examine-the-distribution-of-the-ozone-pollution-data-from-new-york-in-r/
    png(paste('img/plot_normal-histogram_', subject, '.png', sep=''))
    hist(x, breaks=5, freq=FALSE, xlab=xlabel, ylab="Density", col="lightgray",
         main=paste("Histogram of", subject, "\nwith Normal Density Curve\nmean=", round(mean, 2), "sd=", round(sd, 2)))
    curve(dnorm(x, mean=mean, sd=sd), col="red", add=TRUE)
    dev.off()
}


rateMultipliers <- c("1", "1,5", "2")
frequencies <- c(3, 6, 9, 12, 15, 18, 21, 24, 27, 30)
amountsOfTrams <- c(8, 10, 12, 14, 16, 18, 20, 22)

options <- length(rateMultipliers) * length(frequencies) * length(amountsOfTrams)

performanceMeasures <- data.frame(Setting=character(options),
                                  DelayPercentage=numeric(options), 
                                  AverageDelay=numeric(options),
                                  MaximumDelay=numeric(options), 
                                  PassengerDelayPercentage=numeric(options), 
                                  AverageWaitingTime=numeric(options), 
                                  MaximumWaitingTime=numeric(options), 
                                  PassengersTransported=numeric(options), 
                                  MaximumQueueLength=numeric(options), 
                                  AverageQueueLength=numeric(options),
                                  stringsAsFactors=FALSE) 

for (rateMultiplier in 1:length(rateMultipliers))
{
    for (frequency in 1:length(frequencies))
    {
        for (amountOfTrams in 1:length(amountsOfTrams))
        {
            setting <- paste(rateMultipliers[rateMultiplier], "_", "55800-", frequencies[frequency], "-", "300", "-", amountsOfTrams[amountOfTrams], sep="")
            print(setting)
            results <- read.csv(paste("output/measurements_", setting, ".csv", sep=""), sep=";")
            
            colnames(results) <- c("DelayPercentage","AverageDelay","MaximumDelay",
                                   "PassengerDelayPercentage","AverageWaitingTime","MaximumWaitingTime",
                                   "PassengersTransported","MaximumQueueLength","AverageQueueLength")
            
            results$DelayPercentage = sub(",",".",results$DelayPercentage)
            results$DelayPercentage  = as.numeric(results$DelayPercentage)
            results$AverageDelay = sub(",",".",results$AverageDelay)
            results$AverageDelay  = as.numeric(results$AverageDelay)
            results$PassengerDelayPercentage = sub(",",".",results$PassengerDelayPercentage)
            results$PassengerDelayPercentage = as.numeric(results$PassengerDelayPercentage)
            results$AverageWaitingTime = sub(",",".",results$AverageWaitingTime)
            results$AverageWaitingTime = as.numeric(results$AverageWaitingTime)
            
            
            index = ((rateMultiplier - 1) * (length(frequencies) * length(amountsOfTrams))) + ((frequency - 1) * length(amountsOfTrams)) + (amountOfTrams - 1) + 1
            performanceMeasures$Setting[index] <- setting
            performanceMeasures$DelayPercentage[index] <- mean(results$DelayPercentage)
            performanceMeasures$AverageDelay[index] <- mean(results$AverageDelay)
            performanceMeasures$MaximumDelay[index] <- mean(results$MaximumDelay)
            performanceMeasures$PassengerDelayPercentage[index] <- mean(results$PassengerDelayPercentage)
            performanceMeasures$AverageWaitingTime[index] <- mean(results$AverageWaitingTime)
            performanceMeasures$MaximumWaitingTime[index] <- mean(results$MaximumWaitingTime)
            performanceMeasures$PassengersTransported[index] <- mean(results$PassengersTransported)
            performanceMeasures$MaximumQueueLength[index] <- mean(results$MaximumQueueLength)
            performanceMeasures$AverageQueueLength[index] <- mean(results$AverageQueueLength)
            
            
            plotNormalDistribution(results$DelayPercentage, mean(results$DelayPercentage), sd(results$DelayPercentage),
                                   "Delay percentage", paste("delayPercentage of", setting))
            
            plotNormalDistribution(results$AverageDelay, mean(results$AverageDelay), sd(results$AverageDelay),
                                   "Average delay", paste("averageDelay of", setting))
            
            plotNormalDistribution(results$MaximumDelay, mean(results$MaximumDelay), sd(results$MaximumDelay),
                                   "Maximum delay", paste("maximumDelay of", setting))
            
            plotNormalDistribution(results$PassengerDelayPercentage, mean(results$PassengerDelayPercentage), sd(results$PassengerDelayPercentage),
                                   "Passenger delay percentage", paste("passengerDelayPercentage of", setting))
            
            plotNormalDistribution(results$AverageWaitingTime, mean(results$AverageWaitingTime), sd(results$AverageWaitingTime),
                                   "Average waiting time", paste("averageWaitingTime of", setting))
            
            plotNormalDistribution(results$MaximumWaitingTime, mean(results$MaximumWaitingTime), sd(results$MaximumWaitingTime),
                                   "Maximum waiting time", paste("maximumWaitingTime of", setting))
            
            plotNormalDistribution(results$PassengersTransported, mean(results$PassengersTransported), sd(results$PassengersTransported),
                                   "Passengers transported", paste("passengersTransported of", setting))
            
            plotNormalDistribution(results$MaximumQueueLength, mean(results$MaximumQueueLength), sd(results$MaximumQueueLength),
                                   "Maximum queue length", paste("maximumQueueLength of", setting))
            
            plotNormalDistribution(results$AverageQueueLength, mean(results$AverageQueueLength), sd(results$AverageQueueLength),
                                   "Average queue length", paste("averageQueueLength of", setting))
            
        }
    }
}

performanceMeasures[,-1] <-round(performanceMeasures[,-1],2)

write.csv(file="performanceMeasures.csv", x=performanceMeasures)